﻿extern alias resharper;
namespace Simple.Testing.ReSharperRunner.Runners
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Framework;
	using resharper::JetBrains.ReSharper.TaskRunnerFramework;
	using Notifications;
	using Tasks;

	internal class RecursiveTaskRunner : RecursiveRemoteTaskRunner
	{
		//    readonly RemoteTaskNotificationFactory _taskNotificationFactory = new RemoteTaskNotificationFactory();
		private Assembly _contextAssembly;
		private Type _contextClass;
		private PerContextRunListener _listener;
		private RemoteTaskNotificationFactory _taskNotificationFactory = new RemoteTaskNotificationFactory();
		private SpecificationRunner _runner;
		private MemberInfo _member;
		private TaskResult _taskResult;

		public RecursiveTaskRunner(IRemoteTaskServer server)
			: base(server)
		{
		}

		public override TaskResult Start(TaskExecutionNode node)
		{
			var task = (ContextSpecificationTask)node.RemoteTask;

			_contextAssembly = LoadContextAssembly(task);
			if (_contextAssembly == null)
			{
				return TaskResult.Error;
			}

			//			var result = VersionCompatibilityChecker.Check(_contextAssembly);
			//			if (!result.Success)
			//			{
			//				Server.TaskExplain(task, result.Explanation);
			//				Server.TaskError(task, result.ErrorMessage);
			//
			//				return TaskResult.Error;
			//			}

			_contextClass = _contextAssembly.GetType(task.ContextTypeName);
			if (_contextClass == null)
			{
				Server.TaskExplain(task,
								   String.Format("Could not load type '{0}' from assembly {1}.",
												 task.ContextTypeName,
												 task.AssemblyLocation));
				Server.TaskError(node.RemoteTask, "Could not load context");
				return TaskResult.Error;
			}


			_member = _contextClass.GetField(task.SpecificationFieldName, BindingFlags.Instance | BindingFlags.Public);
			
			if(_member == null)
			{
				Server.TaskExplain(task,
								   String.Format("Could not find specification '{0}' in type {1} from assembly {2}.",
									task.SpecificationFieldName,			 
								   task.ContextTypeName,
												 task.AssemblyLocation));
				Server.TaskError(node.RemoteTask, "Could not find specification");
				return TaskResult.Error;
			}

			Server.TaskStarting(task);
			
			return TaskResult.Success;
		}

		public override TaskResult Execute(TaskExecutionNode node)
		{
			// This method is never called.
			return TaskResult.Success;
		}

		public override TaskResult Finish(TaskExecutionNode node)
		{
			return _taskResult;
		}

		public override void ExecuteRecursive(TaskExecutionNode node)
		{
			var remoteTask = node.RemoteTask;
			Server.TaskProgress(remoteTask, null);

			var runResult = SimpleRunner.RunMember(_member);

			
			Server.TaskOutput(remoteTask, runResult.Message, TaskOutputType.STDOUT);
		

			var taskResult = TaskResult.Success;

			var message = string.Empty;
			
			if (!runResult.Passed)
			{
				if (runResult.Thrown != null)
				{
						message += runResult.Message + Environment.NewLine;
						Server.TaskExplain(remoteTask, runResult.Message);
						Server.TaskException(remoteTask, new[] { new TaskException(runResult.Thrown) });
				}
					foreach (var expectation in runResult.Expectations.Where(e => !e.Passed))
					{
						message += expectation.Text + Environment.NewLine;
						Server.TaskExplain(remoteTask, expectation.Text);
						Server.TaskException(remoteTask, new[] { new TaskException(expectation.Exception) });
					}
				
				taskResult = TaskResult.Exception;
			}
			else
			{
			
				foreach (var expectation in runResult.Expectations)
					{
						message += expectation.Text + Environment.NewLine;
						Server.TaskExplain(remoteTask, expectation.Text);
					
					}
				
				
			}

			Server.TaskFinished(remoteTask, message, taskResult);
			_taskResult = taskResult;
		}

		void RegisterRemoteTaskNotifications(TaskExecutionNode node)
		{
			_listener.RegisterTaskNotification(_taskNotificationFactory.CreateTaskNotification(node));
		}

		private static IEnumerable<TaskExecutionNode> FlattenChildren(TaskExecutionNode node)
		{
			foreach (var child in node.Children)
			{
				yield return child;

				foreach (var descendant in child.Children)
				{
					yield return descendant;
				}
			}
		}

		private Assembly LoadContextAssembly(Task task)
		{
			AssemblyName assemblyName;
			if (!File.Exists(task.AssemblyLocation))
			{
				Server.TaskExplain(task,
								   String.Format("Could not load assembly from {0}: File does not exist", task.AssemblyLocation));
				Server.TaskError(task, "Could not load context assembly");
				return null;
			}

			try
			{
				assemblyName = AssemblyName.GetAssemblyName(task.AssemblyLocation);
			}
			catch (FileLoadException ex)
			{
				Server.TaskExplain(task,
								   String.Format("Could not load assembly from {0}: {1}", task.AssemblyLocation, ex.Message));
				Server.TaskError(task, "Could not load context assembly");
				return null;
			}

			if (assemblyName == null)
			{
				Server.TaskExplain(task,
								   String.Format("Could not load assembly from {0}: Not an assembly", task.AssemblyLocation));
				Server.TaskError(task, "Could not load context assembly");
				return null;
			}

			try
			{
				return Assembly.Load(assemblyName);
			}
			catch (Exception ex)
			{
				Server.TaskExplain(task,
								   String.Format("Could not load assembly from {0}: {1}", task.AssemblyLocation, ex.Message));
				Server.TaskError(task, "Could not load context assembly");
				return null;
			}
		}
	}

	public class PerContextRunListener
	{
		readonly RemoteTask _contextTask;
		readonly IRemoteTaskServer _server;
		readonly IList<RemoteTaskNotification> _taskNotifications = new List<RemoteTaskNotification>();

		public PerContextRunListener(IRemoteTaskServer server, RemoteTask contextNode)
		{
			_server = server;
			_contextTask = contextNode;
		}

		public void OnAssemblyStart(AssemblyInfo assembly)
		{
		}

		public void OnAssemblyEnd(AssemblyInfo assembly)
		{
		}

		public void OnRunStart()
		{
		}

		public void OnRunEnd()
		{
		}

		public void OnSpecificationStart(SpecificationInfo specification)
		{
			var notify = CreateTaskNotificationFor(specification);

			notify(task => _server.TaskStarting(task));
			notify(task => _server.TaskProgress(task, "Running specification"));
		}

		public void OnSpecificationEnd(SpecificationInfo specification, RunResult result)
		{
			var notify = CreateTaskNotificationFor(specification);

			notify(task => _server.TaskProgress(task, null));

			notify(task => _server.TaskOutput(task, result.Message, TaskOutputType.STDOUT));
			//notify(task => _server.TaskOutput(task, result.ConsoleError, TaskOutputType.STDERR));

			TaskResult taskResult = TaskResult.Success;
			string message = null;
			if(!result.Passed)
			{
				notify(task => _server.TaskExplain(task, result.Message));
				notify(task => _server.TaskException(task, new []{new TaskException(result.Thrown)}));
				message = result.Thrown.ToString();
				
				taskResult = TaskResult.Exception;
			}

			notify(task => _server.TaskFinished(task, message, taskResult));
		}

		public void OnFatalError(Exception exception)
		{
			_server.TaskExplain(_contextTask, "Fatal error: " + exception.Message);
			_server.TaskException(_contextTask, new[] { new TaskException(exception) });
			_server.TaskFinished(_contextTask, exception.ToString()
				, TaskResult.Exception);
		}

		internal void RegisterTaskNotification(RemoteTaskNotification notification)
		{
			_taskNotifications.Add(notification);
		}

		Action<Action<RemoteTask>> CreateTaskNotificationFor(SpecificationInfo specification)
		{
			return actionToBePerformedForEachTask =>
			{
				foreach (var notification in _taskNotifications)
				{
					if (notification.Matches(specification))
					{
						Debug.WriteLine(String.Format("Notifcation for {0} {1}, with {2} remote tasks",
													  specification.ContainingType,
													  specification.FieldName,
													  notification.RemoteTasks.Count()));

						foreach (var task in notification.RemoteTasks)
						{
							actionToBePerformedForEachTask(task);
						}
					}
				}
			};
		}

		delegate void Action<T>(T arg);
	}

}