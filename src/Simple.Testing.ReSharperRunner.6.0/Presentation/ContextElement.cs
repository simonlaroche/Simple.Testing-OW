using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
#if RESHARPER_61
using JetBrains.ReSharper.UnitTestFramework.Elements;
#endif
using JetBrains.Util;


using ContextFactory = Simple.Testing.ReSharperRunner.Factories.ContextFactory;

namespace Simple.Testing.ReSharperRunner.Presentation
{
	using Factories;

	public class ContextElement : Element, ISerializableElement
  {
    readonly string _assemblyLocation;
    readonly IEnumerable<UnitTestElementCategory> _categories;

    public ContextElement(MSpecUnitTestProvider provider,
                          PsiModuleManager psiModuleManager,
                          CacheManager cacheManager, 
                          ProjectModelElementEnvoy projectEnvoy,
                          string typeName,
                          string assemblyLocation,
                          bool isIgnored)
      : base(provider, psiModuleManager, cacheManager, null, projectEnvoy, typeName, isIgnored)
    {
      _assemblyLocation = assemblyLocation;
    }

    public override string ShortName
    {
      get { return Kind; }
    }

    public string AssemblyLocation
    {
      get { return _assemblyLocation; }
    }

    public override string GetPresentation()
    {
      return new ClrTypeName(GetTypeClrName()).ShortName;
    }

    public override IDeclaredElement GetDeclaredElement()
    {
      return GetDeclaredType();
    }

    public override string Kind
    {
      get { return "Context"; }
    }

    public override IEnumerable<UnitTestElementCategory> Categories
    {
      get { return _categories; }
    }

    public override string Id
    {
      get { return CreateId(TypeName); }
    }

    public void WriteToXml(XmlElement parent)
    {
      parent.SetAttribute("typeName", TypeName);
      parent.GetAttribute("assemblyLocation", AssemblyLocation);
      parent.SetAttribute("isIgnored", Explicit.ToString());
    }

    public static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, MSpecUnitTestProvider provider, ISolution solution
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

      var typeName = parent.GetAttribute("typeName");
      var assemblyLocation = parent.GetAttribute("assemblyLocation");
      var isIgnored = bool.Parse(parent.GetAttribute("isIgnored"));
      var subject = parent.GetAttribute("subject");

      return ContextFactory.GetOrCreateContextElement(provider,
#if RESHARPER_61
                                                      manager, psiModuleManager, cacheManager,
#endif
                                                      project,
                                                      ProjectModelElementEnvoy.Create(project),
                                                      typeName,
                                                      assemblyLocation);
    }

    public static string CreateId(string typeName)
    {
	  System.Diagnostics.Debug.WriteLine("CE  " + typeName);
	  return typeName;
    }
  }
}