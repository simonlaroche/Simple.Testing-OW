extern alias resharper;
using resharper::JetBrains.UI.Application.PluginSupport;
#if RESHARPER_6
using ResharperPluginManager = resharper::JetBrains.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.Application.PluginSupport.PluginVendorAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperSolutionComponentAttribute = resharper::JetBrains.ProjectModel.SolutionComponentAttribute;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IProjectToAssemblyReference;
using ResharperThreading = resharper::JetBrains.Threading.IThreading;
#else
using ResharperPluginManager = resharper::JetBrains.UI.Application.PluginSupport.PluginManager;
using ResharperPlugin = resharper::JetBrains.UI.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginDescriptionAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.UI.Application.PluginSupport.PluginVendorAttribute;
using ResharperThreading = Simple.Testing.ReSharperRunner.IThreading;
using ResharperAssemblyReference = resharper::JetBrains.ProjectModel.IAssemblyReference;

#endif


[assembly: ResharperPluginTitleAttribute("Simple.Testing Resharper Test Runner")]
[assembly: ResharperPluginDescriptionAttribute("Provides integration of Simple.Testing within ReSharper.")]
[assembly: ResharperPluginVendorAttribute("free")]

namespace Simple.Testing.ReSharperRunner
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using EnvDTE;
	using EnvDTE80;
	using resharper::JetBrains.UI.Application.PluginSupport;
	

	/// <summary>
	/// Provides support for dynamic loading and unloading of ReSharper plugins, always lives in the same AppDomain as ReSharper.
	/// </summary>
	public class PluginManager : MarshalByRefObject, IDisposable
	{
		public const string ACTION_REANALYZE = "ErrorsView.ReanalyzeFilesWithErrors";
		public const string OUTPUT_RESHARPER_TESTS = "OpenWrap-Tests";
		readonly DTE2 _dte;
		List<Assembly> _loadedAssemblies = new List<Assembly>();
		bool _resharperLoaded;

		static ResharperPlugin _selfPlugin;
		System.Threading.Thread _debugThread;
		bool runTestRunner = true;
		OpenWrapOutput _output;
		ResharperThreading _threading;

#if RESHARPER_6
        resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost _host;
        resharper::JetBrains.Application.Parts.AssemblyPartsCatalogue _catalog;
        resharper::JetBrains.Application.PluginSupport.PluginsDirectory _pluginsDirectory;
        resharper::JetBrains.DataFlow.LifetimeDefinition _lifetimeDefinition;
#endif

		public const string RESHARPER_TEST = "?ReSharper";

		public PluginManager()
		{
			_dte = (DTE2)SiteManager.GetGlobalService<DTE>();
			_output = new OpenWrapOutput("Resharper Plugin Manager");
			_output.Write("Loaded ({0}).", GetType().Assembly.GetName().Version);

#if !RESHARPER_6
			_threading = new LegacyShellThreading();
#else
			_host = resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost.GetOrCreateHost((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte);
            var resolvedObj = _host.Environment.Container.ResolveDynamic(typeof(ResharperThreading));
            if (resolvedObj != null)
                _threading = (ResharperThreading)resolvedObj.Instance;
#endif
			if (_threading == null)
			{
				_output.Write("Threading not found, the plugin manager will not initialize.");
				return;
			}
			Guard.Run(_threading,"Loading plugins...", StartDetection);

		}

		public void Dispose()
		{
			_output.Write("Unloading.");
			runTestRunner = false;
#if !RESHARPER_6
			_selfPlugin.Enabled = false;
			ResharperPluginManager.Instance.Plugins.Remove(_selfPlugin);
#else
			_selfPlugin.IsEnabled.SetValue(false);
            _pluginsDirectory.Plugins.Remove(_selfPlugin);
            _lifetimeDefinition.Terminate();
            _host = null;
            _catalog = null;
#endif
			_selfPlugin = null;
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}
		void StartDetection()
		{
			try
			{
				var asm = GetType().Assembly;
				var id = "ReSharper OpenWrap Integration";
#if RESHARPER_6
                _lifetimeDefinition = resharper::JetBrains.DataFlow.Lifetimes.Define(resharper::JetBrains.DataFlow.EternalLifetime.Instance, "OpenWrap Solution");
                _pluginsDirectory =
                    (resharper::JetBrains.Application.PluginSupport.PluginsDirectory)_host.Environment.Container.ResolveDynamic(typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory)).Instance;

                _selfPlugin = new ResharperPlugin(_lifetimeDefinition.Lifetime, new[] { new resharper::JetBrains.Util.FileSystemPath(asm.Location) }, null, null, null);
                
                _pluginsDirectory.Plugins.Add(_selfPlugin);
#else
				_selfPlugin = new ResharperPlugin(id, new[] { asm });

				ResharperPluginManager.Instance.Plugins.Add(_selfPlugin);
				_selfPlugin.Enabled = true;
				resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(id, asm);
#endif
			}
			catch (Exception e)
			{
				_output.Write("Plugin integration failed.\r\n" + e.ToString());
			}
		}
	}
}