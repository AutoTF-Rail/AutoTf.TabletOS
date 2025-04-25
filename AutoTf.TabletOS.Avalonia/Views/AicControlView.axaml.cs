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
	
	#endregion
	
	#region LocationKeyboardInteraction
	
	private void ResetLocation()
	{
		CurrentLocationBox.Text = _currentLocation;
		KeyboardArea.IsVisible = false;
		AicInteractButtonsArea.IsVisible = true;
	}
	
	private void CurrentLocationBox_OnGotFocus(object? sender, GotFocusEventArgs e)
	{
		AicInteractButtonsArea.IsVisible = false;
		KeyboardArea.IsVisible = true;
	}

	private async void KeyboardDone_Click(object? sender, RoutedEventArgs e)
	{
		await InvokeLoadingScreen(true, "Saving location...");
		_currentLocation = CurrentLocationBox.Text!;
		AicInteractButtonsArea.IsVisible = true;
		KeyboardArea.IsVisible = false;

		await InvokeLoadingScreen(false);
	}

	private void KeyboardCancel_Click(object? sender, RoutedEventArgs e)
	{
		ResetLocation();
	}

	private void KeyboardBack_Click(object? sender, RoutedEventArgs e)
	{
		string currString = CurrentLocationBox.Text!;
		CurrentLocationBox.Text = currString.Substring(0, currString.Length - 1);
	}

	private void CurrentLocationBox_OnTextChanged(object? sender, TextChangedEventArgs e)
	{
		if (sender is TextBox textBox)
		{
			textBox.CaretIndex = textBox.Text?.Length ?? 0;
		}
	}

	private void AddCharToLocation(string character)
	{
		CurrentLocationBox.Text = CurrentLocationBox.Text += character;
	}

	private void KeyboardButtonComma_Click(object? sender, RoutedEventArgs e)
	{
		if (CurrentLocationBox.Text!.Contains(","))
			return;
		AddCharToLocation(",");
	}

	private void KeyboardButton0_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("0");

	private void KeyboardButton1_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("1");

	private void KeyboardButton2_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("2");

	private void KeyboardButton3_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("3");

	private void KeyboardButton4_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("4");

	private void KeyboardButton5_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("5");

	private void KeyboardButton6_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("6");

	private void KeyboardButton7_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("7");

	private void KeyboardButton8_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("8");

	private void KeyboardButton9_Click(object? sender, RoutedEventArgs e) => AddCharToLocation("9");
	#endregion
}