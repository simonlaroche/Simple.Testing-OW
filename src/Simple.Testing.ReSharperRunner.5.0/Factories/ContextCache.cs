extern alias resharper;
using System.Collections.Generic;

using resharper::JetBrains.ReSharper.Psi;

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
