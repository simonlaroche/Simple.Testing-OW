using System.Collections.Generic;

using JetBrains.ReSharper.Psi;

using Simple.Testing.ReSharperRunner.Presentation;

namespace Simple.Testing.ReSharperRunner.Factories
{
	using Presentation;

	internal class ContextCache
  {
    public ContextCache()
    {
      Classes = new Dictionary<ITypeElement, ContextElement>();
    }

    public IDictionary<ITypeElement, ContextElement> Classes
    {
      get;
      private set;
    }
  }
}
