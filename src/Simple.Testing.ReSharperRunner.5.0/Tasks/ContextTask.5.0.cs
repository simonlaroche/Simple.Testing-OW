extern alias resharper;
using System;

using resharper::JetBrains.ReSharper.TaskRunnerFramework;

namespace Simple.Testing.ReSharperRunner.Tasks
{
	internal partial class ContextTask : IUnitTestRemoteTask 
	{
		public string TypeName
		{
			get { return ContextTypeName; }
		}

		public string ShortName
		{
			get { return String.Empty; }
		}
	}
}