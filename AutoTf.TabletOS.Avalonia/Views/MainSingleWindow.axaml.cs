using System;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainSingleWindow : UserControl, IUiControl
{
	public MainSingleWindow()
	{
		InitializeComponent();
	}
	
	public void SetView(UserControl view)
	{
		ActiveView.Content = view;
	}
    
	public void ShowLoadingScreen(bool visible, string text = "")
	{
		LoadingName.Text = text;
		LoadingArea.IsVisible = visible;
	}

	public async Task<int> AddDialog<T>(ViewBase<T> dialog)
	{
		DialogStack.Children.Add(dialog);
		// _mainWindow.DialogHost.Content = dialog;
		// _mainWindow.DialogHost.IsVisible = true;

		if (dialog.DataContext is not DialogViewModelBase viewModel)
			throw new Exception("Given View did not have a DialogViewModelBase.");

		int result = await viewModel.ShowAsync();

		DialogStack.Children.Remove(dialog);

		return result;
	}

	public int DialogCount()
	{
		return DialogStack.Children.Count;
	}
}