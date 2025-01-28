using System.IO;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
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

	private void Update_Click(object? sender, RoutedEventArgs e)
	{
		if (!NetworkManager.IsInternetAvailable())
			return;

		string prevDir = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
		Statics.ExecuteCommand("eval $(\"ssh-agent\")");
		Statics.ExecuteCommand("ssh-add /home/display/githubKey");
		Statics.ExecuteCommand("git pull");
		Statics.ExecuteCommand("dotnet build -c RELEASE -m");
		Statics.ExecuteCommand("reboot now");
	}
}