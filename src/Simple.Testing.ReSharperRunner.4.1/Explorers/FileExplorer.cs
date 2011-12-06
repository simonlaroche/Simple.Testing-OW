using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestExplorer;

using Simple.Testing.ReSharperRunner.Explorers.ElementHandlers;
using Simple.Testing.ReSharperRunner.Factories;

namespace Simple.Testing.ReSharperRunner.Explorers
{
	using ElementHandlers;
	using Factories;

	internal class FileExplorer : IRecursiveElementProcessor
  {
    readonly UnitTestElementLocationConsumer _consumer;
    readonly IEnumerable<IElementHandler> _elementHandlers;
    readonly IFile _file;
    readonly CheckForInterrupt _interrupted;

    public FileExplorer(IUnitTestProvider provider,
                        UnitTestElementLocationConsumer consumer,
                        IFile file,
                        CheckForInterrupt interrupted)
    {
      if (file == null)
      {
        throw new ArgumentNullException("file");
      }

      if (provider == null)
      {
        throw new ArgumentNullException("provider");
      }

      _consumer = consumer;
      _file = file;
      _interrupted = interrupted;

      IProject project = file.ProjectFile.GetProject();
      string assemblyPath = UnitTestManager.GetOutputAssemblyPath(project).FullPath;

      var cache = new ContextCache();
      var contextFactory = new ContextFactory(provider, project, assemblyPath, cache);
      var contextSpecificationFactory = new ContextSpecificationFactory(provider, project, cache);

      _elementHandlers = new List<IElementHandler>
                         {
                           new ContextElementHandler(contextFactory),
                           new ContextSpecificationElementHandler(contextSpecificationFactory),
                         };
    }

    public bool InteriorShouldBeProcessed(IElement element)
    {
      if (element is ITypeMemberDeclaration)
      {
        return element is ITypeDeclaration;
      }

      return true;
    }

    public void ProcessBeforeInterior(IElement element)
    {
      IElementHandler handler = _elementHandlers.Where(x => x.Accepts(element)).FirstOrDefault();
      if (handler == null)
      {
        return;
      }

      foreach (var elementDisposition in handler.AcceptElement(element, _file))
      {
        if (elementDisposition != null && elementDisposition.UnitTestElement != null)
        {
          _consumer(elementDisposition);
        }
      }
    }

    public void ProcessAfterInterior(IElement element)
    {
    }

    public bool ProcessingIsFinished
    {
      get
      {
        if (_interrupted())
        {
          throw new ProcessCancelledException();
        }

        return false;
      }
    }
  }
}
