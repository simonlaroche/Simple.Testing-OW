using JetBrains.ReSharper.TaskRunnerFramework;
#if RESHARPER_5
using JetBrains.ReSharper.UnitTestFramework;
#else
using JetBrains.ReSharper.UnitTestExplorer;
#endif

#if RESHARPER_6
using JetBrains.ReSharper.UnitTestFramework;
#endif

using Simple.Testing.ReSharperRunner.Presentation;
using Simple.Testing.ReSharperRunner.Tasks;

namespace Simple.Testing.ReSharperRunner.Factories
{
	using Presentation;
	using Tasks;

	internal class UnitTestTaskFactory
	{
		readonly string _providerId;

		public UnitTestTaskFactory(string providerId)
		{
			_providerId = providerId;
		}

		public UnitTestTask CreateAssemblyLoadTask(ContextElement context)
		{
			return new UnitTestTask(null,
									new AssemblyLoadTask(context.AssemblyLocation));
		}

		public UnitTestTask CreateContextTask(ContextElement context, bool isExplicit)
		{
			return new UnitTestTask(context,
									new ContextTask(_providerId,
													context.AssemblyLocation,
													context.GetTypeClrName(),
													false));
		}

		public UnitTestTask CreateContextSpecificationTask(ContextElement context,
														   ContextSpecificationElement contextSpecification,
														   bool isExplicit)
		{
			return new UnitTestTask(contextSpecification,
									new ContextSpecificationTask(_providerId,
																 context.AssemblyLocation,
																 context.GetTypeClrName(),
																 contextSpecification.FieldName,
																 false));
		}
	}
}