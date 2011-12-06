//using System.Collections.Generic;
//
//using JetBrains.ReSharper.TaskRunnerFramework;
//
//using Simple.Testing.ReSharperRunner.Tasks;
//using Simple.Testing.Utility;
//using Simple.Testing.Utility.Internal;
//
//namespace Simple.Testing.ReSharperRunner.Runners.Notifications
//{
//	using Tasks;
//
//	internal class BehaviorSpecificationRemoteTaskNotification : RemoteTaskNotification
//  {
//    readonly TaskExecutionNode _node;
//    readonly BehaviorSpecificationTask _task;
//
//    public BehaviorSpecificationRemoteTaskNotification(TaskExecutionNode node)
//    {
//      _node = node;
//      _task = (BehaviorSpecificationTask) node.RemoteTask;
//    }
//
//    protected override string ContainingType
//    {
//      get { return _task.BehaviorTypeName; }
//    }
//
//    protected override string Name
//    {
//      get { return _task.SpecificationFieldName.ToFormat(); }
//    }
//
//    public override IEnumerable<RemoteTask> RemoteTasks
//    {
//      get
//      {
//        yield return _node.RemoteTask;
//      }
//    }
//  }
//}