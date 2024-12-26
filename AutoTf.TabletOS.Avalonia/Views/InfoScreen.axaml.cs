using AutoTf.TabletOS.Avalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class InfoScreen : UserControl
{
	private readonly object _previousControl;

	public InfoScreen(object previousControl)
	{
		_previousControl = previousControl;
		InitializeComponent();

		Initialize();
	}

	private void Initialize()
	{
		VersionBox.Text = "Version: " + Program.GetGitVersion();
	}

	private void Button_OnClick(object? sender, RoutedEventArgs e)
	{
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = _previousControl;
		}
	}
}