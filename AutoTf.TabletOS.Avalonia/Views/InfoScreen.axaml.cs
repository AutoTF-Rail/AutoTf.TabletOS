using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class InfoScreen : UserControl
{
	private readonly UserControl _previousControl;
	private readonly INetworkService _networkService = Statics.NetworkService;
	private readonly Logger _logger = Statics.Logger;

	public InfoScreen(UserControl previousControl)
	{
		_previousControl = previousControl;

		InitializeComponent();

		Initialize();
	}

	private void Initialize()
	{
		VersionBox.Text = "Version: " + Program.GetGitVersion();
		UpdateButton.IsEnabled = NetworkService.IsInternetAvailable();
		LoadingArea.IsVisible = false;
	}

	private void Button_OnClick(object? sender, RoutedEventArgs e)
	{
		Dispatcher.UIThread.Invoke(() => Statics.ChangeViewModel.Invoke(_previousControl));
	}

	private async void Update_Click(object? sender, RoutedEventArgs e)
	{
		if (!NetworkService.IsInternetAvailable())
			return;

		LoadingArea.IsVisible = true;
		LoadingName.Text = "Updating...";
		InfoOutput.Text = "";
		await Dispatcher.UIThread.InvokeAsync(() => UpdateText.IsVisible = true);
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
		
		CommandExecuter.ExecuteCommand("git reset --hard");
		
		LoadingName.Text = "Downloading updates...";
		await Task.Delay(50);
		
		string pull = CommandExecuter.ExecuteCommand("git pull");
		
		if (pull.Contains("Already"))
		{
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				InfoOutput.Text = "";
				InfoStatus.Text = "Already Up to Date.";
			});
			await Task.Delay(50);
			
			CommandExecuter.ExecuteSilent("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh", true);
			LoadingArea.IsVisible = false;
			return;
		}
		CommandExecuter.ExecuteCommand("git submodule update --init --recursive");
		
		LoadingName.Text = "Building update...";
		await Task.Delay(50);
		
		string buildOutput = CommandExecuter.ExecuteCommand("dotnet build -c RELEASE -m");
		if (!buildOutput.Contains("0 Error(s)"))
		{
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				InfoOutput.Text = "";
				InfoStatus.Text = "Failed to build the update.";
				LoadingArea.IsVisible = false;
			});
			return;
		}
		await Task.Delay(50);
		
		CommandExecuter.ExecuteCommand("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh");
		_networkService.ShutdownConnection();
		
		LoadingName.Text = "Rebooting...";
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoStatus.Text = "";
			
			CommandExecuter.ExecuteSilent("reboot now", true);
		});
		await Task.Delay(50);
		
		Directory.SetCurrentDirectory(prevDir);
		LoadingArea.IsVisible = false;
	}

	private void RebootButton_OnClick(object? sender, RoutedEventArgs e)
	{
		try
		{
			_networkService.ShutdownConnection();
		}
		catch (Exception exception)
		{
			_logger.Log("Failed to shutdown connection:");
			_logger.Log(exception.ToString());
		}
		finally
		{
			CommandExecuter.ExecuteSilent("reboot now", true);
		}
	}

	private void LogsButton_OnClick(object? sender, RoutedEventArgs e)
	{
		LogsViewer logsViewer = new LogsViewer();
		logsViewer.Show(RootGrid);
	}
}