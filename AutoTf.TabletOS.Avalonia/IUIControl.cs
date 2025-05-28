using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia;

public interface IUiControl
{
    public void SetView(UserControl view);

    public void ShowLoadingScreen(bool visible, string text = "");

    public Task<int> AddDialog<T>(ViewBase<T> dialog);
    public int DialogCount();
}