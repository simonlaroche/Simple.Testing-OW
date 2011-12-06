using JetBrains.CommonControls;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;

using Simple.Testing.ReSharperRunner.Presentation;

namespace Simple.Testing.ReSharperRunner
{
	using Presentation;

	[UnitTestPresenter]
  public class MSpecUnitTestPresenter : IUnitTestPresenter 
  {
    Presenter _presenter;

    public MSpecUnitTestPresenter() { _presenter = new Presenter(); }
      
    public void Present(IUnitTestElement element, IPresentableItem item, TreeModelNode node, PresentationState state)
    {
      if (element is Element)
      {
        _presenter.UpdateItem(element, node, item, state);
      }
    }
  }
}