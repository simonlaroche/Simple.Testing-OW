using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestExplorer;

namespace Simple.Testing.ReSharperRunner.Presentation
{
	internal class ContextSpecificationElement : FieldElement
	{
		public ContextSpecificationElement(IUnitTestProvider provider,
			// ReSharper disable SuggestBaseTypeForParameter
										   ContextElement context,
			// ReSharper restore SuggestBaseTypeForParameter
										   IProjectModelElement project,
										   string declaringTypeName,
										   string fieldName)
			: base(provider, context, project, declaringTypeName, fieldName)
		{
		}

		public ContextElement Context
		{
			get { return (ContextElement)Parent; }
		}

		public override string GetKind()
		{
			return "Specification";
		}
	}
}