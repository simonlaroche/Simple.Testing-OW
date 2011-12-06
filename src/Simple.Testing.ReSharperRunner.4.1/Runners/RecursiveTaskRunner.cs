namespace Simple.Testing.ReSharperRunner.Runners
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Framework;
	using JetBrains.ReSharper.TaskRunnerFramework;
	using Tasks;

	internal class RecursiveTaskRunner : RecursiveRemoteTaskRunner
	{
//    readonly RemoteTaskNotificationFactory _taskNotificationFactory = new RemoteTaskNotificationFactory();
		private Assembly _contextAssembly;
		private Type _contextClass;

		public RecursiveTaskRunner(IRemoteTaskServer server) : base(server)
		{
		}

		public override TaskResult Start(TaskExecutionNode node)
		{
			var task = (ContextTask) node.RemoteTask;

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

			return TaskResult.Success;
		}

		public override TaskResult Execute(TaskExecutionNode node)
		{
			// This method is never called.
			return TaskResult.Success;
		}

		public override TaskResult Finish(TaskExecutionNode node)
		{
			SimpleRunner.RunMember(_contextClass);

			return TaskResult.Success;
		}

		public override void ExecuteRecursive(TaskExecutionNode node)
		{
//		foreach (var VARIABLE in FlattenChildren(node))
//    	{
//    		VARIABLE.
//    	}
//      FlattenChildren(node).Each(RegisterRemoteTaskNotifications);
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
}