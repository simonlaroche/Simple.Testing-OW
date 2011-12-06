//using System.Collections.Generic;
//
//using JetBrains.ReSharper.TaskRunnerFramework;
//
//namespace Simple.Testing.ReSharperRunner.Runners.Notifications
//{
//  internal abstract class RemoteTaskNotification
//  {
//    protected abstract string ContainingType
//    {
//      get;
//    }
//
//    protected abstract string Name
//    {
//      get;
//    }
//
//    public abstract IEnumerable<RemoteTask> RemoteTasks
//    {
//      get;
//    }
//
//    public virtual bool Matches(SpecificationInfo specification)
//    {
//      return ContainingType == specification.ContainingType &&
//             Name == specification.Name;
//    }
//  }
//}