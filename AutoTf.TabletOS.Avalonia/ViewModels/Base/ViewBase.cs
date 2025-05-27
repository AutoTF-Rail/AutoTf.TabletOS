using Autofac;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Base;

public abstract class ViewBase<TViewModel> : UserControl
{
    public ViewBase()
    {
        if (!Design.IsDesignMode)
        {
            TViewModel vm = App.Container.Resolve<TViewModel>();
            DataContext = vm;
        }
    }
    
    public ViewBase(TViewModel viewModel)
    {
        DataContext = viewModel;
    }
}