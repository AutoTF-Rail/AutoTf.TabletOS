using System;
using System.Threading.Tasks;
using Autofac;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia;

public class ViewRouter : IViewRouter
{
    private readonly IUiControl _uiControl;
    private readonly ILifetimeScope _scope;

    public ViewRouter(IUiControl uiControl, ILifetimeScope scope)
    {
        _uiControl = uiControl;
        _scope = scope;
    }

    public void NavigateTo<TView>() where TView : UserControl
    {
        TView view = _scope.Resolve<TView>();
        _uiControl.SetView(view);
    }

    public void NavigateTo(UserControl view)
    {
        _uiControl.SetView(view);
    }

    public async Task<int> ShowDialog<T>(ViewBase<T> dialog)
    {
        return await _uiControl.AddDialog(dialog);
    }

    public async Task<int> ShowDialog<TView, TViewModel>() where TView : ViewBase<TViewModel> where TViewModel : DialogViewModelBase
    {
        TView view = _scope.Resolve<TView>();
        
        return await ShowDialog(view);
    }

    public void InvokeLoadingArea(bool visible, string message = "")
    {
        _uiControl.ShowLoadingScreen(visible, message);
    }
}