using System;
using System.Threading.Tasks;
using Autofac;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.Views;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia;

public class ViewRouter : IViewRouter
{
    private readonly MainWindow _mainWindow;
    private readonly ILifetimeScope _scope;

    public ViewRouter(MainWindow mainWindow, ILifetimeScope scope)
    {
        _mainWindow = mainWindow;
        _scope = scope;
    }

    public void NavigateTo<TView>() where TView : UserControl
    {
        TView view = _scope.Resolve<TView>();
        _mainWindow.SetView(view);
    }

    public void NavigateTo(UserControl view)
    {
        _mainWindow.SetView(view);
    }

    public async Task<int> ShowDialog<T>(ViewBase<T> dialog)
    {
        _mainWindow.DialogStack.Children.Add(dialog);
        // _mainWindow.DialogHost.Content = dialog;
        // _mainWindow.DialogHost.IsVisible = true;

        if (dialog.DataContext is not DialogViewModelBase viewModel)
            throw new Exception("Given View did not have a DialogViewModelBase.");

        int result = await viewModel.ShowAsync();

        _mainWindow.DialogStack.Children.Remove(dialog);

        return result;
    }

    public async Task<int> ShowDialog<TView, TViewModel>() where TView : ViewBase<TViewModel> where TViewModel : DialogViewModelBase
    {
        TView view = _scope.Resolve<TView>();
        
        return await ShowDialog(view);
    }

    public void InvokeLoadingArea(bool visible, string message = "")
    {
        _mainWindow.ShowLoadingScreen(visible, message);
    }
}