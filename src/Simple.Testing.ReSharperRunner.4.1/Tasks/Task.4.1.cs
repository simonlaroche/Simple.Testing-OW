using JetBrains.ReSharper.TaskRunnerFramework;

namespace Simple.Testing.ReSharperRunner.Tasks
{
  internal partial class Task
  {
    bool BaseEquals(RemoteTask other)
    {
      return Equals(other);
    }
  }
}