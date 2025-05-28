using System;
using System.Threading.Tasks;
using Autofac;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Threading;

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

    public async Task NavigateTo<TView>() where TView : UserControl
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TView view = _scope.Resolve<TView>();
                _uiControl.SetView(view);

                Dispatcher.UIThread.Post(() =>
                {
                    if (view.DataContext is ViewModelBase vm)
                    {
                        vm.InitializeAsync();
                    }
                }, DispatcherPriority.Background);
            }, DispatcherPriority.Render);

            await Task.Delay(25);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in NavigateTo: " + e);
        }
    }

    public async Task NavigateTo(UserControl view)
    {
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _uiControl.SetView(view);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _uiControl.SetView(view);
                }, DispatcherPriority.Render);
            }
            await Task.Delay(25);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in NavigateTo: " + e);
        }
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

    public int DialogCount()
    {
        return _uiControl.DialogCount();
    }

    public async Task InvokeLoadingArea(bool visible, string message = "")
    {
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _uiControl.ShowLoadingScreen(visible, message);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                        _uiControl.ShowLoadingScreen(visible, message),
                    DispatcherPriority.Render);
            }
            
            await Task.Delay(25).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in InvokeLoadingArea: " + e);
        }
    }
}