﻿extern alias resharper;
namespace Simple.Testing.ReSharperRunner.Runners.Notifications
{
	using resharper::JetBrains.ReSharper.TaskRunnerFramework;
	using Tasks;


	internal class RemoteTaskNotificationFactory
	{
		public RemoteTaskNotification CreateTaskNotification(TaskExecutionNode node)
		{
			var remoteTask = node.RemoteTask;

			if (remoteTask is ContextSpecificationTask)
			{
				return new ContextSpecificationRemoteTaskNotification(node);
			}
			
			return new SilentRemoteTaskNotification();
		}
	}
}