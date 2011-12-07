using System;
using System.Linq;

using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
#if RESHARPER_61
using JetBrains.ReSharper.UnitTestFramework.Elements;
#endif
using JetBrains.Util;

using Simple.Testing.ReSharperRunner.Factories;

namespace Simple.Testing.ReSharperRunner.Explorers
{
	using Factories;
	using Framework;

	class AssemblyExplorer
  {
    readonly IMetadataAssembly _assembly;
    readonly UnitTestElementConsumer _consumer;
    readonly ContextFactory _contextFactory;
    readonly ContextSpecificationFactory _contextSpecificationFactory;

    public AssemblyExplorer(MSpecUnitTestProvider provider,
#if RESHARPER_61
                            IUnitTestElementManager manager,
                            PsiModuleManager psiModuleManager,
                            CacheManager cacheManager,
#endif
                            IMetadataAssembly assembly,
                            IProject project,
                            UnitTestElementConsumer consumer)
    {
      _assembly = assembly;
      _consumer = consumer;

      using (ReadLockCookie.Create())
      {
        var projectEnvoy = new ProjectModelElementEnvoy(project);

        var cache = new ContextCache();
#if RESHARPER_61
        _contextFactory = new ContextFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy, _assembly.Location.FullPath, cache);
        _contextSpecificationFactory = new ContextSpecificationFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy, cache);
        _behaviorFactory = new BehaviorFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy, cache);
        _behaviorSpecificationFactory = new BehaviorSpecificationFactory(provider, manager, psiModuleManager, cacheManager, project, projectEnvoy);
#else
#if RESHARPER_6
        _contextFactory = new ContextFactory(provider, project, projectEnvoy, _assembly.Location.FullPath, cache);
#else
        _contextFactory = new ContextFactory(provider, project, projectEnvoy, _assembly.Location, cache);
#endif
        _contextSpecificationFactory = new ContextSpecificationFactory(provider, project, projectEnvoy, cache);
#endif
      }
    }

    public void Explore()
    {
      if (!_assembly.ReferencedAssembliesNames.Any(x => String.Equals(
#if RESHARPER_6
                                                          x.Name,
#else
                                                          x.AssemblyName.Name,
#endif
                                                          typeof(Specification).Assembly.GetName().Name,
                                                          StringComparison.InvariantCultureIgnoreCase)))
      {
        return;
      }

      CollectionUtil.ForEach(_assembly.GetTypes().Where(type => type.IsContext()), type =>
      {
        var contextElement = _contextFactory.CreateContext(type);
        _consumer(contextElement);

        CollectionUtil.ForEach(type
                   	.GetSpecifications(), x => _consumer(_contextSpecificationFactory.CreateContextSpecification(contextElement, x)));
      });
    }
  }
}