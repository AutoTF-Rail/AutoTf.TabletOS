using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class InfoScreen : UserControl
{
	private readonly object _previousControl;
	private readonly NetworkManager _networkManager = Statics.NetworkManager;
	private readonly Logger _logger = Statics.Logger;

	public InfoScreen(object previousControl)
	{
		_previousControl = previousControl;

		InitializeComponent();

		Initialize();
	}

	private void Initialize()
	{
		VersionBox.Text = "Version: " + Program.GetGitVersion();
		UpdateButton.IsVisible = NetworkManager.IsInternetAvailable();
	}

	private void Button_OnClick(object? sender, RoutedEventArgs e)
	{
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = _previousControl;
		}
	}

	private async void Update_Click(object? sender, RoutedEventArgs e)
	{
		if (!NetworkManager.IsInternetAvailable())
			return;

		Dispatcher.UIThread.InvokeAsync(() => UpdateText.IsVisible = true);
		await Task.Delay(50);
		
		string prevDir = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			UpdateText.IsVisible = true;
			InfoStatus.IsVisible = true;
			InfoOutput.IsVisible = true;
		});
		await Task.Delay(50);

		string evalOutput = CommandExecuter.ExecuteCommand("eval $(\"ssh-agent\")");
		_logger.Log(evalOutput);
		
		await Dispatcher.UIThread.InvokeAsync(() =>InfoOutput.Text = evalOutput);
		await Task.Delay(50);

		string add = CommandExecuter.ExecuteCommand("ssh-add /home/display/githubKey");
		_logger.Log(add);
		await Dispatcher.UIThread.InvokeAsync(() => InfoOutput.Text = add);
		await Task.Delay(50);
		
		string aasddd = CommandExecuter.ExecuteCommand("git reset --hard");
		_logger.Log(aasddd);
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = aasddd;
			InfoStatus.Text = "Pulling";
		});
		await Task.Delay(50);
		
		string pull = CommandExecuter.ExecuteCommand("git pull");
		_logger.Log(pull);
		if (pull.Contains("Already"))
		{
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				InfoOutput.Text = "";
				InfoStatus.Text = "Already Up to Date.";
			});
			await Task.Delay(50);
			
			CommandExecuter.ExecuteSilent("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh", true);
			return;
		}
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = pull;
			InfoStatus.Text = "Building";
		});
		await Task.Delay(50);
		
		string build = CommandExecuter.ExecuteCommand("dotnet build -c RELEASE -m");
		_logger.Log(build);
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = build;
			InfoStatus.Text = "Perms";
		});
		await Task.Delay(50);
		
		string perms = CommandExecuter.ExecuteCommand("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh");
		_logger.Log(perms);
		
		_networkManager.ShutdownConnection();
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = perms;
		
			InfoStatus.Text = "Reboot";
			CommandExecuter.ExecuteSilent("reboot now", true);
		});
		await Task.Delay(50);
		
		Directory.SetCurrentDirectory(prevDir);
	}

	private void RebootButton_OnClick(object? sender, RoutedEventArgs e)
	{
		_networkManager.ShutdownConnection();
		CommandExecuter.ExecuteSilent("reboot now", true);
	}
}