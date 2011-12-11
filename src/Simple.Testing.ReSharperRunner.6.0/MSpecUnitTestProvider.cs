extern alias resharper;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Xml;

using resharper::JetBrains.ProjectModel;
using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Caches;
using resharper::JetBrains.ReSharper.TaskRunnerFramework;
using resharper::JetBrains.ReSharper.UnitTestFramework;
#if RESHARPER_61
using resharper::JetBrains.ReSharper.UnitTestFramework.Elements;
#endif

using Simple.Testing.ReSharperRunner.Presentation;
using Simple.Testing.ReSharperRunner.Properties;
using Simple.Testing.ReSharperRunner.Runners;

namespace Simple.Testing.ReSharperRunner
{
	using Presentation;
	using Properties;
	using Runners;

	[UnitTestProvider]
  public class MSpecUnitTestProvider : IUnitTestProvider
  {
    const string ProviderId = "Simple.Testing";
    readonly UnitTestElementComparer _unitTestElementComparer = new UnitTestElementComparer();
    private UnitTestManager _unitTestManager;

#if RESHARPER_61
    public MSpecUnitTestProvider()
    {
#else
    public MSpecUnitTestProvider(ISolution solution, PsiModuleManager psiModuleManager, CacheManager cacheManager)
    {
      Solution = solution;
      PsiModuleManager = psiModuleManager;
      CacheManager = cacheManager;
#endif
      Debug.Listeners.Add(new DefaultTraceListener());
    }

#if !RESHARPER_61
    public PsiModuleManager PsiModuleManager { get; private set; }
    public CacheManager CacheManager { get; private set; }

    public UnitTestManager UnitTestManager
    {
      get { return _unitTestManager ?? (_unitTestManager = Solution.GetComponent<UnitTestManager>());  }
    }
#endif

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

    public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
    {
    }

    public void ExploreExternal(UnitTestElementConsumer consumer)
    {
    }

#if !RESHARPER_61
    public ISolution Solution { get; private set; }

    public void SerializeElement(XmlElement parent, IUnitTestElement element)
    {
      var e = element as ISerializableElement;
      if (e != null)
      {
        e.WriteToXml(parent);
        parent.SetAttribute("elementType", e.GetType().Name);
      }
    }

    public IUnitTestElement DeserializeElement(XmlElement parent, IUnitTestElement parentElement)
    {
      var typeName = parent.GetAttribute("elemenType");

      if (Equals(typeName, "ContextElement"))
        return ContextElement.ReadFromXml(parent, parentElement, this, Solution);
      if (Equals(typeName, "ContextSpecificationElement"))
        return ContextSpecificationElement.ReadFromXml(parent, parentElement, this, Solution);

      return null;
    }
#endif

    public RemoteTaskRunnerInfo GetTaskRunnerInfo()
    {
      return new RemoteTaskRunnerInfo(typeof(RecursiveTaskRunner));
    }

    public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
    {
      return _unitTestElementComparer.Compare(x, y);
    }

    public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
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

    public bool IsSupported(IHostProvider hostProvider)
    {
      return true;
    }
  }
}