extern alias resharper;
using System.Collections.Generic;

using resharper::JetBrains.ProjectModel;
using resharper::JetBrains.ReSharper.UnitTestFramework;

namespace Simple.Testing.ReSharperRunner.Presentation
{
  internal class ContextSpecificationElement : FieldElement
  {
    public ContextSpecificationElement(IUnitTestProvider provider,
                                       // ReSharper disable SuggestBaseTypeForParameter
                                       ContextElement context,
                                       // ReSharper restore SuggestBaseTypeForParameter
										ProjectModelElementEnvoy project,
                                       string declaringTypeName,
                                       string fieldName,
                                       bool isIgnored)
      : base(provider, context, project, declaringTypeName, fieldName, isIgnored || context.IsExplicit)
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
