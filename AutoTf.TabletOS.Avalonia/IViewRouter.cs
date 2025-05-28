using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia;

public interface IViewRouter
{
    public Task NavigateTo<TView>() where TView : UserControl;
    public Task NavigateTo(UserControl view);
    public Task<int> ShowDialog<T>(ViewBase<T> dialog);
    public Task<int> ShowDialog<TView, TViewModel>() where TView : ViewBase<TViewModel> where TViewModel : DialogViewModelBase;
    public Task InvokeLoadingArea(bool visible, string message = "");
}