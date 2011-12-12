extern alias resharper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using resharper::JetBrains.ProjectModel;
using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Caches;
using resharper::JetBrains.ReSharper.UnitTestFramework;
#if RESHARPER_61
using resharper::JetBrains.ReSharper.UnitTestFramework.Elements;
#endif
using resharper::JetBrains.Util;

using Simple.Testing.ReSharperRunner.Factories;

namespace Simple.Testing.ReSharperRunner.Presentation
{
	using Factories;

	internal class ContextSpecificationElement : FieldElement
  {
		public ContextSpecificationElement(TestProvider provider,
                                       PsiModuleManager psiModuleManager,
                                       CacheManager cacheManager, 
      // ReSharper disable SuggestBaseTypeForParameter
                                       ContextElement context,
      // ReSharper restore SuggestBaseTypeForParameter
                                       ProjectModelElementEnvoy project,
                                       string declaringTypeName,
                                       string fieldName, bool isIgnored)
      : base(provider, psiModuleManager, cacheManager, context, project, declaringTypeName, fieldName, isIgnored || context.Explicit)
    {
    }

    public ContextElement Context
    {
      get { return (ContextElement)Parent; }
    }

    public override string Kind
    {
      get {  return "Specification"; }
    }

		public override IEnumerable<UnitTestElementCategory> Categories
		{
			get { return Enumerable.Empty<UnitTestElementCategory>(); }
		}

		public static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, TestProvider provider, ISolution solution
#if RESHARPER_61
      , IUnitTestElementManager manager, PsiModuleManager psiModuleManager, CacheManager cacheManager
#endif
      )
    {
      var projectId = parent.GetAttribute("projectId");
      var project = ProjectUtil.FindProjectElementByPersistentID(solution, projectId) as IProject;
      if (project == null)
      {
        return null;
      }

      var context = parentElement as ContextElement;
      if (context == null)
      {
        return null;
      }

      var typeName = parent.GetAttribute("typeName");
      var methodName = parent.GetAttribute("methodName");
      var isIgnored = bool.Parse(parent.GetAttribute("isIgnored"));

      return ContextSpecificationFactory.GetOrCreateContextSpecification(provider,
#if RESHARPER_61
                manager, psiModuleManager, cacheManager,
#endif
                project, context, ProjectModelElementEnvoy.Create(project), typeName, methodName);
    }

    public override string Id
    {
      get { return CreateId(Context, FieldName); }
    }

    public static string CreateId(ContextElement parent, string fieldName)
    {
      var id = String.Format("{0}.{1}", parent.Id, fieldName);
      System.Diagnostics.Debug.WriteLine("CSE " + id);
      return id;
    }
  }
}
