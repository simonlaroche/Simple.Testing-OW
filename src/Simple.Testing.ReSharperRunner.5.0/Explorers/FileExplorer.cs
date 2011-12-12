extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;

using resharper::JetBrains.Application;
using resharper::JetBrains.Application.Progress;
using resharper::JetBrains.ProjectModel;
using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Caches;
using resharper::JetBrains.ReSharper.Psi.Tree;
using resharper::JetBrains.ReSharper.UnitTestFramework;
#if RESHARPER_61
using resharper::JetBrains.ReSharper.UnitTestFramework.Elements;
#endif

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

    public FileExplorer(TestProvider provider,
#if RESHARPER_61
                        IUnitTestElementManager manager,
                        PsiModuleManager psiModuleManager,
                        CacheManager cacheManager,
#endif
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

#if RESHARPER_6
      IProject project =  PsiSourceFileExtensions.ToProjectFile(file.GetSourceFile()).GetProject();
#else
      resharper::JetBrains.ProjectModel.IProject project = file.ProjectFile.GetProject();
#endif
      var projectEnvoy = new ProjectModelElementEnvoy(project);
      string assemblyPath = UnitTestManager.GetOutputAssemblyPath(project).FullPath;

      var cache = new ContextCache();


#if RESHARPER_61
      var contextFactory = new ContextFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy, assemblyPath, cache);
      var contextSpecificationFactory = new ContextSpecificationFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy, cache);
      var behaviorFactory = new BehaviorFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy, cache);
      var behaviorSpecificationFactory = new BehaviorSpecificationFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy);
#else
      var contextFactory = new ContextFactory(provider, project, projectEnvoy, assemblyPath, cache);
      var contextSpecificationFactory = new ContextSpecificationFactory(provider, project, projectEnvoy, cache);
#endif

      _elementHandlers = new List<IElementHandler>
                         {
                           new ContextElementHandler(contextFactory),
                           new ContextSpecificationElementHandler(contextSpecificationFactory),
                         };
    }

#if RESHARPER_6
    public bool InteriorShouldBeProcessed(ITreeNode element)
#else 
    public bool InteriorShouldBeProcessed(IElement element)
#endif
    {
      if (element is ITypeMemberDeclaration)
      {
        return element is ITypeDeclaration;
      }

      return true;
    }

#if RESHARPER_6
    public void ProcessBeforeInterior(ITreeNode element)
#else
    public void ProcessBeforeInterior(IElement element)
#endif 
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

#if RESHARPER_6
    public void ProcessAfterInterior(ITreeNode element)
    {
      _elementHandlers
        .Where(x => x.Accepts(element))
        .ToList()
        .ForEach(x => x.Cleanup(element));
    }
#else
    public void ProcessAfterInterior(IElement element)
    {
    }
#endif

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
