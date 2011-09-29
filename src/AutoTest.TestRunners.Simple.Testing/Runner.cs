namespace AutoTest.TestRunners.Simple.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Shared;
	using Shared.AssemblyAnalysis;
	using Shared.Communication;
	using Shared.Logging;
	using Shared.Options;
	using Shared.Results;
	using global::Simple.Testing.Framework;

	public class Runner:IAutoTestNetTestRunner
	{
		private ITestFeedbackProvider _channel;
		private Func<string, IReflectionProvider> _reflectionProviderFactory = (assembly) => Reflect.On(assembly);
		private ILogger _logger;
		
		public void SetLogger(ILogger logger)
		{
			_logger = logger;
		}

		public void SetReflectionProvider(Func<string, IReflectionProvider> reflectionProviderFactory)
		{
			_reflectionProviderFactory = reflectionProviderFactory;
		}

		public void SetLiveFeedbackChannel(ITestFeedbackProvider channel)
		{
			_channel = channel;
		}

		public bool IsTest(string assembly, string member)
		{
			using (var locator = _reflectionProviderFactory(assembly))
			{
				var fixture = locator.LocateClass(member);
				if (fixture == null)
					return false;
				return !fixture.IsAbstract && (
					fixture.Fields.Count(x =>
					x.FieldType == "Simple.Testing.Framework.Specification") > 0);
			}
		}

		public bool ContainsTestsFor(string assembly, string member)
		{
			return IsTest(assembly, member);
		}

		public bool ContainsTestsFor(string assembly)
		{
			using (IReflectionProvider provider = _reflectionProviderFactory(assembly))
			{
				return provider.GetReferences().Contains("Simple.Testing.Framework");
			}
		}
		
		public bool Handles(string identifier)
		{
			return identifier.ToLower().Equals(Identifier.ToLower());
		}

		public IEnumerable<TestResult> Run(RunSettings settings)
		{
			return new StfuRunner(_logger, _reflectionProviderFactory, _channel).Run(settings);
			
		}

		public string Identifier
		{
			get { return "Stfu"; }
		}
	}

	public class StfuRunner
	{
		private string identifier = "Stfu";

		private readonly ILogger logger;
		private readonly Func<string, IReflectionProvider> reflectionProviderFactory;
		private readonly ITestFeedbackProvider channel;

		public StfuRunner(ILogger logger, Func<string, IReflectionProvider> reflectionProviderFactory, ITestFeedbackProvider channel)
		{
			this.logger = logger;
			this.reflectionProviderFactory = reflectionProviderFactory;
			this.channel = channel;
		}

		public IEnumerable<TestResult> Run(RunSettings settings)
		{
			var results = SimpleRunner.RunAllInAssembly(settings.Assembly.Assembly);

			return results.Select(
				x =>
				new TestResult(identifier, settings.Assembly.Assembly, x.SpecificationName, 0, x.FoundOnMemberInfo.Name, "",
				               x.Passed == true ? TestState.Passed : TestState.Failed, x.Message)).ToArray();

		}
	}
}