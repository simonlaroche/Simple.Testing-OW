//using JetBrains.ReSharper.TaskRunnerFramework;
//
//using Simple.Testing.ReSharperRunner.Tasks;
//
//namespace Simple.Testing.ReSharperRunner.Runners.Notifications
//{
//	using Tasks;
//
//	internal class RemoteTaskNotificationFactory
//  {
//    public RemoteTaskNotification CreateTaskNotification(TaskExecutionNode node)
//    {
//      var remoteTask = node.RemoteTask;
//
//      if (remoteTask is ContextSpecificationTask)
//      {
//        return new ContextSpecificationRemoteTaskNotification(node);
//      }
//
//      if (remoteTask is BehaviorSpecificationTask)
//      {
//        return new BehaviorSpecificationRemoteTaskNotification(node);
//      }
//
//      return new SilentRemoteTaskNotification();
//    }
//  }
//}