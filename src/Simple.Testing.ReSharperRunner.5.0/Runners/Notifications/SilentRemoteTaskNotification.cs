extern alias resharper;
using System.Collections.Generic;

using resharper::JetBrains.ReSharper.TaskRunnerFramework;

namespace Simple.Testing.ReSharperRunner.Runners.Notifications
{
  internal class SilentRemoteTaskNotification : RemoteTaskNotification
  {
    protected override string ContainingType
    {
      get { return null; }
    }

    protected override string Name
    {
      get { return null; }
    }

    public override IEnumerable<RemoteTask> RemoteTasks
    {
      get { yield break; }
    }
  }
}