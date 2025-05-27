using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using AutoTf.TabletOS.Models;
using Avalonia.Input;

namespace AutoTf.TabletOS.Avalonia.Views.Dialog;

public partial class AicControlView : ViewBase<AicControlViewModel>
{
	public AicControlView()
	{
		InitializeComponent();
	}
	
	private async void CurrentLocationBox_OnGotFocus(object? sender, GotFocusEventArgs e)
	{
		(bool success, string? result) = await Statics.ShowKeyboard(CurrentLocationBox.Text ?? "", 8);
		
		if (!success)
			return;

		CurrentLocationBox.Text = result;

		// TODO: Send new location to Central Bridge But maybe do this in the viewmodel again
	}
}