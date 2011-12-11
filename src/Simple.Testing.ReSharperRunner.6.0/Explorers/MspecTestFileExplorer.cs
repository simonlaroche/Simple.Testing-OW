extern alias resharper;
using resharper::JetBrains.Application;
using resharper::JetBrains.ReSharper.Psi;
using resharper::JetBrains.ReSharper.Psi.Caches;
using resharper::JetBrains.ReSharper.Psi.Tree;
using resharper::JetBrains.ReSharper.UnitTestFramework;
#if RESHARPER_61
using resharper::JetBrains.ReSharper.UnitTestFramework.Elements;
#endif

namespace Simple.Testing.ReSharperRunner.Explorers
{
  [FileUnitTestExplorer] 
  public class MspecTestFileExplorer : IUnitTestFileExplorer
  {
    readonly MSpecUnitTestProvider _provider;
#if RESHARPER_61
    readonly IUnitTestElementManager _manager;
    readonly PsiModuleManager _psiModuleManager;
    readonly CacheManager _cacheManager;
#endif

#if RESHARPER_61
    public MspecTestFileExplorer(MSpecUnitTestProvider provider, IUnitTestElementManager manager, PsiModuleManager psiModuleManager, CacheManager cacheManager)
    {
      _manager = manager;
      _psiModuleManager = psiModuleManager;
      _cacheManager = cacheManager;
#else
    public MspecTestFileExplorer(MSpecUnitTestProvider provider)
    {
#endif
      _provider = provider;
    }

    public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
    {
      if ((psiFile.Language.Name == "CSHARP") || (psiFile.Language.Name == "VBASIC"))
      {
        psiFile.ProcessDescendants(new FileExplorer(_provider,
#if RESHARPER_61
          _manager, _psiModuleManager, _cacheManager, 
#endif
          consumer, psiFile, interrupted));
      }
    }

    public IUnitTestProvider Provider { get { return _provider; }}
  }
}