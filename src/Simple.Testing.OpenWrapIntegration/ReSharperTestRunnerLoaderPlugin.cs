namespace Simple.Testing.OpenWrapIntegration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using OpenFileSystem.IO;
	using OpenWrap.Reflection;
	using OpenWrap.Resources;
	using OpenWrap.Services;

	public class ReSharperTestRunnerLoaderPlugin : IDisposable
	{
		private const int MAX_RETRIES = 50;

		private static readonly IDictionary<Version, string> versionToLoaders = new Dictionary<Version, string>
		                                                                        	{
		                                                                        		{
		                                                                        			new Version("5.0.1659.36"),
		                                                                        			"Simple.Testing.ResharperRunner.PluginManager, Simple.Testing.ResharperRunner.5.0"
		                                                                        			},
		                                                                        		{
		                                                                        			new Version("5.1.1727.12"),
		                                                                        			"Simple.Testing.ResharperRunner.PluginManager, Simple.Testing.ResharperRunner.5.1"
		                                                                        			},
		                                                                        		{                
		                                                                        			new Version("6.0.2202.688"),
		                                                                        			"Simple.Testing.ReSharperRunner.PluginManager, Simple.Testing.ReSharperRunner.6.0"
		                                                                        			},
		                                                                        	};

		private readonly IFileSystem _fileSystem;

		private readonly IDirectory _rootAssemblyCacheDir;
		private OpenWrapOutput _output;
		private object _pluginManager;
		private int _retries;

		public ReSharperTestRunnerLoaderPlugin() : this(ServiceLocator.GetService<IFileSystem>())
		{
		}

		public ReSharperTestRunnerLoaderPlugin(IFileSystem fileSystem)
		{
			_output = new OpenWrapOutput();
			
			_output.Write(
				"Starting resharper test runner plugin");
			_fileSystem = fileSystem;
			_rootAssemblyCacheDir =
				_fileSystem.GetDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).GetDirectory(
					"openwrap").GetDirectory("VisualStudio").GetDirectory("AssemblyCache").MustExist();

			var vsAppDomain = AppDomain.CurrentDomain.GetData("openwrap.vs.appdomain") as AppDomain;
			if (vsAppDomain == null)
			{
				_output.Write(
				"vs AppDomain is null");
			
				return;
			}

			DetectResharperLoading(vsAppDomain);
		}

		public void Dispose()
		{
			var disposer = _pluginManager as IDisposable;
			if (disposer != null)
			{
				disposer.Dispose();
			}
		}

		private static Type LoadTypeFromVersion(Version resharperAssembly)
		{
			var typeName = (from version in versionToLoaders.Keys
			                orderby version descending
			                where resharperAssembly >= version
			                select versionToLoaders[version]).FirstOrDefault();
			return typeName != null ? Type.GetType(typeName, true) : null;
		}

		private IFile CopyAndSign(string assemblyLocation)
		{
			var sourceAssemblyFile = _fileSystem.GetFile(assemblyLocation);
			// cheat because we know that the package will *always* be in /name-0.0.0.0/solution/xxxx
			var cacheDir = _rootAssemblyCacheDir.GetDirectory(sourceAssemblyFile.Parent.Parent.Name);
			if (cacheDir.Exists)
			{
				return cacheDir.GetFile(sourceAssemblyFile.Name);
			}
			var destinationAssemblyFile = cacheDir.GetFile(sourceAssemblyFile.Name);
			var now = DateTime.UtcNow;
			var build = (int) (now - new DateTime(2011, 06, 28)).TotalDays;

			var revision = (int) (now - new DateTime(now.Year, now.Month, now.Day)).TotalSeconds;

			sourceAssemblyFile.Sign(destinationAssemblyFile, new StrongNameKeyPair(Keys.openwrap),
			                        new Version(99, 99, build, revision));
			return destinationAssemblyFile;
		}

		private void DetectResharperLoading(AppDomain vsAppDomain)
		{
			while (_retries <= MAX_RETRIES)
			{
				Assembly resharperAssembly;
				try
				{
					resharperAssembly = vsAppDomain.Load("JetBrains.Platform.ReSharper.Shell");
				}
				catch
				{
					resharperAssembly = null;
				}
				if (resharperAssembly == null && _retries++ <= MAX_RETRIES)
				{
					_output.Write("ReSharper not found, try {0}/{1}, sleeping for 2 seconds.", _retries, MAX_RETRIES);
					Thread.Sleep(TimeSpan.FromSeconds(2));
					continue;
				}
				var resharperVersion = resharperAssembly.GetName().Version;
				var pluginManagerType = LoadTypeFromVersion(resharperVersion);
				if (pluginManagerType == null)
				{
					_output.Write("Plug in manager type not found for resharper version {0} .", resharperVersion);
					
					return;
				}

				var assemblyLocation = pluginManagerType.Assembly.Location;
				var destinationAssemblyFile = CopyAndSign(assemblyLocation);

				_pluginManager = vsAppDomain.CreateInstanceFromAndUnwrap(destinationAssemblyFile.Path, pluginManagerType.FullName);
				return;
			}
		}
	}
}