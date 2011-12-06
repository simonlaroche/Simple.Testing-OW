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
//	internal class ContextSpecificationRemoteTaskNotification : RemoteTaskNotification
//  {
//    readonly TaskExecutionNode _node;
//    readonly ContextSpecificationTask _task;
//
//    public ContextSpecificationRemoteTaskNotification(TaskExecutionNode node)
//    {
//      _node = node;
//      _task = (ContextSpecificationTask) node.RemoteTask;
//    }
//
//    protected override string ContainingType
//    {
//      get { return _task.ContextTypeName; }
//    }
//
//    protected override string Name
//    {
//      get { return _task.SpecificationFieldName.ToFormat(); }
//    }
//
//    public override IEnumerable<RemoteTask> RemoteTasks
//    {
//      get { yield return _node.RemoteTask; }
//    }
//  }
//}