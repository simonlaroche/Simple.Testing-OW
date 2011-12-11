extern alias resharper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using resharper::JetBrains.Application;
using resharper::JetBrains.CommonControls;
using resharper::JetBrains.Metadata.Reader.API;
using resharper::JetBrains.ProjectModel;
using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Tree;
using resharper::JetBrains.ReSharper.TaskRunnerFramework;
#if RESHARPER_6
using resharper::JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using resharper::JetBrains.ReSharper.UnitTestExplorer;
using System.Xml;
#endif
using resharper::JetBrains.ReSharper.UnitTestFramework;
using resharper::JetBrains.ReSharper.UnitTestFramework.UI;
using resharper::JetBrains.ReSharper.UnitTestExplorer;
using resharper::JetBrains.TreeModels;
using resharper::JetBrains.UI.TreeView;
using resharper::JetBrains.Util;

using Simple.Testing.ReSharperRunner.Explorers;
using Simple.Testing.ReSharperRunner.Factories;
using Simple.Testing.ReSharperRunner.Presentation;
using Simple.Testing.ReSharperRunner.Properties;
using Simple.Testing.ReSharperRunner.Runners;

namespace Simple.Testing.ReSharperRunner
{
	using Explorers;
	using Factories;
	using Presentation;
	using Properties;
	using Runners;

	[UnitTestProviderAttribute]
  internal class MSpecUnitTestProvider : resharper::JetBrains.ReSharper.UnitTestFramework.IUnitTestProvider
  {
    const string ProviderId = "Simple.Testing";
    static readonly Presenter Presenter = new Presenter();
    readonly UnitTestTaskFactory _taskFactory = new UnitTestTaskFactory(ProviderId);
    readonly UnitTestElementComparer _unitTestElementComparer = new UnitTestElementComparer();

    public MSpecUnitTestProvider()
    {
      Debug.Listeners.Add(new DefaultTraceListener());
    }

    public string ID
    {
      get { return ProviderId; }
    }

    public string Name
    {
      get { return ID; }
    }

    public Image Icon
    {
      get { return null; }
    }

#if RESHARPER_6
    public string Serialize(XmlElement element)
#else
    public string Serialize(UnitTestElement element)
#endif
    {
      return null;
    }

#if RESHARPER_6
    public XmlElement Deserialize(ISolution solution, string elementString)
#else
    public UnitTestElement Deserialize(ISolution solution, string elementString)
#endif
    {
      return null;
    }

    public void ProfferConfiguration(TaskExecutorConfiguration configuration, UnitTestSession session)
    {
    }

    public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
    {
    }

    public void ExploreAssembly(IMetadataAssembly assembly, IProject project, UnitTestElementConsumer consumer)
    {
      AssemblyExplorer explorer = new AssemblyExplorer(this, assembly, project, consumer);
      explorer.Explore();
    }

    public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer,CheckForInterrupt interrupted)
    {
      if (psiFile == null)
      {
        throw new ArgumentNullException("psiFile");
      }

      RecursiveElementProcessorExtensions.ProcessDescendants(psiFile, new FileExplorer(this, consumer, psiFile, interrupted));
    }

    public ProviderCustomOptionsControl GetCustomOptionsControl(ISolution solution)
    {
      return null;
    }

    public void ExploreExternal(UnitTestElementConsumer consumer)
    {
    }

    public RemoteTaskRunnerInfo GetTaskRunnerInfo()
    {
      return new RemoteTaskRunnerInfo(typeof(RecursiveTaskRunner));
    }

    public IList<UnitTestTask> GetTaskSequence(UnitTestElement element, IList<UnitTestElement> explicitElements)
    {
      if (element is ContextSpecificationElement)
      {
        var contextSpecification = element as ContextSpecificationElement;
        var context = contextSpecification.Context;

        return new List<UnitTestTask>
               {
                 _taskFactory.CreateAssemblyLoadTask(context),
                 //_taskFactory.CreateContextTask(context, explicitElements.Contains(context)),
                 _taskFactory.CreateContextSpecificationTask(context,
                                                             contextSpecification,
                                                             explicitElements.Contains(contextSpecification))
               };
      }

      if (element is ContextElement)
      {
        return EmptyArray<UnitTestTask>.Instance;
      }

      throw new ArgumentException(String.Format("Element is not a Machine.Specifications element: '{0}'", element));
    }

    public int CompareUnitTestElements(UnitTestElement x, UnitTestElement y)
    {
      return _unitTestElementComparer.Compare(x, y);
    }

    public bool IsElementOfKind(UnitTestElement element, UnitTestElementKind elementKind)
    {
      switch (elementKind)
      {
        case UnitTestElementKind.Test:
          return element is ContextSpecificationElement;
        case UnitTestElementKind.TestContainer:
          return element is ContextElement;
      }

      return false;
    }

    public void Present(UnitTestElement element, IPresentableItem item,TreeModelNode node, PresentationState state)
    {
      Presenter.UpdateItem(element, node, item, state);
    }

    public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind)
    {
      switch (elementKind)
      {
        case UnitTestElementKind.Test:
          return declaredElement.IsSpecification();
        case UnitTestElementKind.TestContainer:
          return declaredElement.IsContext();
      }

      return false;
    }
  }
}
