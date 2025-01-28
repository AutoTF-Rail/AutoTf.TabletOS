using System.IO;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class InfoScreen : UserControl
{
	private readonly object _previousControl;

	public InfoScreen(object previousControl)
	{
		_previousControl = previousControl;

		UpdateButton.IsVisible = NetworkManager.IsInternetAvailable();
		
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

		Dispatcher.UIThread.Invoke(() => UpdateText.IsVisible = true);

		string prevDir = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
		Dispatcher.UIThread.Invoke(() =>
		{
			InfoOutput.Text = Statics.ExecuteCommand("eval $(\"ssh-agent\")");
			InfoOutput.Text = Statics.ExecuteCommand("ssh-add /home/display/githubKey");
			InfoStatus.Text = "Pulling";

			InfoOutput.Text = Statics.ExecuteCommand("git pull");
			InfoStatus.Text = "Building";
			
			InfoOutput.Text = Statics.ExecuteCommand("dotnet build -c RELEASE -m");
			
			InfoStatus.Text = "Reboot";
			InfoOutput.Text = Statics.ExecuteCommand("reboot now");
		});
	}
}