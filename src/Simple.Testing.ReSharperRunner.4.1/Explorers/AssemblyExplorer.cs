using System;
using System.Linq;

using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.Util;

using Simple.Testing.ReSharperRunner.Factories;
using Simple.Testing.ReSharperRunner.Presentation;

namespace Simple.Testing.ReSharperRunner.Explorers
{
	using Factories;
	using Framework;
	using Presentation;

	internal class AssemblyExplorer
  {
    readonly IMetadataAssembly _assembly;
    readonly UnitTestElementConsumer _consumer;
    readonly ContextFactory _contextFactory;
    readonly ContextSpecificationFactory _contextSpecificationFactory;
    readonly IProject _project;
    readonly UnitTestProvider _provider;

    public AssemblyExplorer(UnitTestProvider provider,
                            IMetadataAssembly assembly,
                            IProject project,
                            UnitTestElementConsumer consumer)
    {
      _provider = provider;
      _assembly = assembly;
      _project = project;
      _consumer = consumer;

      var cache = new ContextCache();
      _contextFactory = new ContextFactory(_provider, _project, _assembly.Location, cache);
      _contextSpecificationFactory = new ContextSpecificationFactory(_provider, _project, cache);
    }

    public void Explore()
    {
      if (!_assembly.ReferencedAssembliesNames.Exists(x => String.Equals(x.AssemblyName.Name,
																		 typeof(Specification).Assembly.GetName().Name,
                                                                         StringComparison.InvariantCultureIgnoreCase)))
      {
        return;
      }

      CollectionUtil.ForEach(_assembly.GetTypes()
                 	.Where(type => type.IsContext()), type =>
          {
            var contextElement = _contextFactory.CreateContext(type);
            _consumer(contextElement);

            CollectionUtil.ForEach(type.GetSpecifications(), x => _consumer(_contextSpecificationFactory.CreateContextSpecification(contextElement, x)));
          });
    }
  }
}
