extern alias resharper;
#if RESHARPER_6
using ResharperPlugin = resharper::JetBrains.Application.PluginSupport.Plugin;
using ResharperPluginTitleAttribute = resharper::JetBrains.Application.PluginSupport.PluginTitleAttribute;
using ResharperPluginVendorAttribute = resharper::JetBrains.Application.PluginSupport.PluginVendorAttribute;
using ResharperPluginDescriptionAttribute = resharper::JetBrains.Application.PluginSupport.PluginDescriptionAttribute;
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
	using EnvDTE;
	using EnvDTE80;

	/// <summary>
	/// Provides support for dynamic loading and unloading of ReSharper plugins, always lives in the same AppDomain as ReSharper.
	/// </summary>
	public class PluginManager : MarshalByRefObject, IDisposable
	{
		readonly DTE2 _dte;

		static ResharperPlugin _selfPlugin;
		OpenWrapOutput _output;
		ResharperThreading _threading;

#if RESHARPER_6
        resharper::JetBrains.VsIntegration.Application.JetVisualStudioHost _host;
		resharper::JetBrains.Application.PluginSupport.PluginsDirectory _pluginsDirectory;
        resharper::JetBrains.DataFlow.LifetimeDefinition _lifetimeDefinition;
#endif

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
#if !RESHARPER_6
			_selfPlugin.Enabled = false;
			ResharperPluginManager.Instance.Plugins.Remove(_selfPlugin);
#else
			_selfPlugin.IsEnabled.SetValue(false);
            _pluginsDirectory.Plugins.Remove(_selfPlugin);
            _lifetimeDefinition.Terminate();
            _host = null;
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
				_output.Write("Start detection of Simple.Testing Resharper plugin" );
				var asm = GetType().Assembly;
			
#if RESHARPER_6
                _lifetimeDefinition = resharper::JetBrains.DataFlow.Lifetimes.Define(resharper::JetBrains.DataFlow.EternalLifetime.Instance, "Simple.Testing Reharper Plugin");
                _pluginsDirectory =
                    (resharper::JetBrains.Application.PluginSupport.PluginsDirectory)_host.Environment.Container.ResolveDynamic(typeof(resharper::JetBrains.Application.PluginSupport.PluginsDirectory)).Instance;

                _selfPlugin = new ResharperPlugin(_lifetimeDefinition.Lifetime, new[] { new resharper::JetBrains.Util.FileSystemPath(asm.Location) }, null, null, null);
                
                _pluginsDirectory.Plugins.Add(_selfPlugin);
#else
				var id = "Simple.Testing ReSharper Plugin";
                _selfPlugin = new ResharperPlugin(id, new[] { asm });

				ResharperPluginManager.Instance.Plugins.Add(_selfPlugin);
				_selfPlugin.Enabled = true;
				resharper::JetBrains.Application.Shell.Instance.LoadAssemblies(id, asm);
#endif
                _output.Write("End detection of Simple.Testing Resharper plugin");
				
			}
			catch (Exception e)
			{
				_output.Write("Plugin integration failed.\r\n" + e.ToString());
			}
		}
	}
}