using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DialogResult = AutoTf.TabletOS.Models.Enums.DialogResult;
using Statics = AutoTf.TabletOS.Models.Statics;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TopBar : UserControl
{
	private readonly UserControl _previousControl;
	private DispatcherTimer _timer = null!;
	private int _brightness;
	private readonly INetworkService _networkService = Statics.NetworkService;
	
	public TopBar()
	{
		InitializeComponent();
		Initialize();
	}

	public void Initialize()
	{
		NotificationsBox.ItemsSource = Statics.Notifications;
		QuickMenuGrid.IsVisible = false;
		Statics.Notifications.CollectionChanged +=
			(_, _) => NotificationsNumber.Text = new string('•', Statics.Notifications.Count);
		
#if RELEASE
		_brightness = int.Parse(File.ReadAllText("/sys/class/backlight/10-0045/brightness"));
#endif
		// LastSynced.Text = "Last Synced: " + Statics.DataManager.GetLastSynced();

		
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1) 
		};
		_timer.Tick += UpdateClock;
		_timer.Start();

		UpdateClock(null, null!);
	}
	private async void UpdateClock(object? sender, EventArgs e)
	{
		Bluber.Text = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
	}

	private void ToggleQuickMenu(object? sender, PointerReleasedEventArgs e)
	{
		QuickMenuGrid.IsVisible = !QuickMenuGrid.IsVisible;
	}

	private async void Shutdown_Click(object? sender, RoutedEventArgs e)
	{
		Popup popup = new Popup("Are you sure you want to shutdown?");
		
		if (await popup.Show(RootGrid) != DialogResult.Yes)
			return;
		
		Statics.Shutdown?.Invoke();
		_networkService.ShutdownConnection();
		
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "shutdown",
				Arguments = "now",
				CreateNoWindow = true,
				UseShellExecute = false
			}
		};
		
		process.Start();
		Environment.Exit(0);
	}

	private async void RestartButton_Click(object? sender, RoutedEventArgs e)
	{
		Popup popup = new Popup("Are you sure you want to restart?");
		
		if (await popup.Show(RootGrid) != DialogResult.Yes)
			return;
		
		Statics.Shutdown?.Invoke();
		_networkService.ShutdownConnection();
		
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "restart",
				Arguments = "now",
				CreateNoWindow = true,
				UseShellExecute = false
			}
		};
		
		process.Start();
		Environment.Exit(0);
	}

	private void DarkerButton_Click(object? sender, RoutedEventArgs e)
	{
		if (_brightness - 25 <= 50)
			return;
		_brightness -= 25;
#if RELEASE
		CommandExecuter.ExecuteSilent("echo " + _brightness.ToString() +
		                       " | sudo tee /sys/class/backlight/10-0045/brightness", true);
#endif
	}

	private void BrighterButton_Click(object? sender, RoutedEventArgs e)
	{
		if (_brightness + 25 >= 255)
			return;
		_brightness += 25;
#if RELEASE
		CommandExecuter.ExecuteSilent("echo " + _brightness.ToString() +
		                              " | sudo tee /sys/class/backlight/10-0045/brightness", true);
#endif
	}

	private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
	{
		if (DataContext is MainWindowViewModel viewModel && viewModel.ActiveView is UserControl uc)
			Dispatcher.UIThread.Invoke(() => Statics.ChangeViewModel.Invoke(new InfoScreen(uc)));
	}

	private void NotificationDiscard_Click(object? sender, RoutedEventArgs e)
	{
		Notification notification = (Notification)((Button)sender!).DataContext!;
		Statics.Notifications.Remove(notification);
	}

	private void NotificationsBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		NotificationsBox.SelectedItem = null;
	}
}