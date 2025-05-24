using System;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using TaskCompletionSource = System.Threading.Tasks.TaskCompletionSource;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class AicControlView : UserControl
{
	private readonly Logger _logger = Statics.Logger;
	private readonly IAicService _aicService = Statics.AicService;
	
	private TaskCompletionSource _taskCompletionSource = null!;
	private Grid _parent = null!;

	private string _currentLocation = "0";
	
	public AicControlView()
	{
		InitializeComponent();
		KeyboardArea.IsVisible = false;
		
		InitializeData();
	}

	private async void InitializeData()
	{
		try
		{
			await InvokeLoadingScreen(true);

			AicInformation aicInformation = new AicInformation();
			await aicInformation.UpdateState();

			AicStatusBox.Text = aicInformation.State;
			AicStatusBox.Foreground = aicInformation.Color;

			CurrentLocationBox.Text = _currentLocation;

			if (!await _aicService.IsOnline())
			{
				InteractionsArea.IsEnabled = false;
				return;
			}

			VersionBox.Text = await _aicService.Version();
		}
		catch (Exception e)
		{
			_logger.Log("An error occured while loading the AIC data:");
			_logger.Log(e.ToString());
			Statics.Notifications.Add(new Notification("An error occured while loading some data. Please view the logs to learn more.",
				Colors.Yellow));
		}
		finally
		{
			await InvokeLoadingScreen(false);
		}
	}

	public Task Show(Grid parent)
	{
		_parent = parent;
		_taskCompletionSource = new TaskCompletionSource();
		parent.Children.Add(this);

		return _taskCompletionSource.Task;
	}
	
	private async Task InvokeLoadingScreen(bool visible, string additionalText = "Loading data...")
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = additionalText;
			LoadingArea.IsVisible = visible;
		});
	}
	
	#region UIEvents
	
	
	private void BackButton_Click(object? sender, RoutedEventArgs e)
	{
		_parent.Children.Remove(this);
		_taskCompletionSource.TrySetResult();
	}

	private async void ShutdownButton_Click(object? sender, RoutedEventArgs e)
	{
		await InvokeLoadingScreen(true, "Shutting down AIC...");
		// TODO: Update status afterwards. (Wait for x seconds or listen for aic unavailablity)
		_aicService.Shutdown();
		await InvokeLoadingScreen(false);
	}

	private async void RestartButton_Click(object? sender, RoutedEventArgs e)
	{
		await InvokeLoadingScreen(true, "Restarting AIC...");
		_aicService.Restart();
		await InvokeLoadingScreen(false);
	}

	private async void LogsButton_Click(object? sender, RoutedEventArgs e)
	{
		string[] logDates = await _aicService.LogDates();
		
		RemoteLogsViewer logsView = new RemoteLogsViewer(logDates, async s => await _aicService.Logs(s));
		await logsView.Show(RootGrid);
	}
	
	#endregion
	
	private async void CurrentLocationBox_OnGotFocus(object? sender, GotFocusEventArgs e)
	{
		(bool success, string? result) = await Statics.ShowKeyboard(CurrentLocationBox.Text ?? "");
		
		if (!success)
			return;

		CurrentLocationBox.Text = result;

		// TODO: Send new location to Central Bridge
	}
}