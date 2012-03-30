extern alias resharper;
namespace Simple.Testing.ReSharperRunner.Runners
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Framework;
	using resharper::JetBrains.ReSharper.TaskRunnerFramework;
	using Tasks;

	internal class RecursiveTaskRunner : RecursiveRemoteTaskRunner
	{
		//    readonly RemoteTaskNotificationFactory _taskNotificationFactory = new RemoteTaskNotificationFactory();
		private Assembly _contextAssembly;
		private Type _contextClass;
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


			_member = _contextClass.GetMethod(task.SpecificationFieldName, BindingFlags.Instance | BindingFlags.Public);
			
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

			var methodName = _member.DeclaringType.FullName + "." + _member.Name;
			var runResult = SimpleRunner.RunByName(_member.DeclaringType.Assembly, methodName).First();
			
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