using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

namespace AutoTf.TabletOS.Avalonia.Views.Dialog;

public partial class Popup : ViewBase<PopupViewModel>
{
	public Popup()
	{
		InitializeComponent();
	}

	public Popup(PopupViewModel vm) : base(vm)
	{
		InitializeComponent();
	}
}