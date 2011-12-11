extern alias resharper;

namespace Simple.Testing.ReSharperRunner
{
	public interface IThreading
	{
		resharper::JetBrains.Threading.JetDispatcher Dispatcher { get; }
		resharper::JetBrains.Threading.ReentrancyGuard ReentrancyGuard { get; }
	}

	public class LegacyShellThreading : IThreading
	{
		public resharper::JetBrains.Threading.JetDispatcher Dispatcher
		{
			get { return resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard.Dispatcher; }
		}

		public resharper::JetBrains.Threading.ReentrancyGuard ReentrancyGuard
		{
			get { return resharper::JetBrains.Application.Shell.Instance.Invocator.ReentrancyGuard; }
		}
	}

}