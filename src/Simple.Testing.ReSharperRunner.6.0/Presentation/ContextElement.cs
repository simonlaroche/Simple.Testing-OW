extern alias resharper;
#if RESHARPER_61
using resharper::JetBrains.ReSharper.UnitTestFramework.Elements;
#endif

namespace Simple.Testing.ReSharperRunner.Presentation
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Xml;
	using Factories;
	using resharper::JetBrains.ProjectModel;
	using resharper::JetBrains.ReSharper.Psi;
	using resharper::JetBrains.ReSharper.Psi.Caches;
	using resharper::JetBrains.ReSharper.UnitTestFramework;

	public class ContextElement : Element, ISerializableElement
	{
		private readonly string assemblyLocation;

		public ContextElement(MSpecUnitTestProvider provider, PsiModuleManager psiModuleManager, CacheManager cacheManager,
		                      ProjectModelElementEnvoy projectEnvoy, string typeName, string assemblyLocation, bool isIgnored)
			: base(provider, psiModuleManager, cacheManager, null, projectEnvoy, typeName, isIgnored)
		{
			this.assemblyLocation = assemblyLocation;
		}

		public override string ShortName
		{
			get { return Kind; }
		}

		public string AssemblyLocation
		{
			get { return assemblyLocation; }
		}

		public override string Kind
		{
			get { return "Context"; }
		}

		public override IEnumerable<UnitTestElementCategory> Categories
		{
			get { return Enumerable.Empty<UnitTestElementCategory>(); }
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

		public override string GetPresentation()
		{
			return new ClrTypeName(GetTypeClrName()).ShortName;
		}

		public override IDeclaredElement GetDeclaredElement()
		{
			return GetDeclaredType();
		}

		public static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement,
		                                           MSpecUnitTestProvider provider, ISolution solution
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
			
			return ContextFactory.GetOrCreateContextElement(provider,
#if RESHARPER_61
                                                      manager, psiModuleManager, cacheManager,
#endif
			                                                project, ProjectModelElementEnvoy.Create(project), typeName,
			                                                assemblyLocation);
		}

		public static string CreateId(string typeName)
		{
			Debug.WriteLine("CE  " + typeName);
			return typeName;
		}
	}
}