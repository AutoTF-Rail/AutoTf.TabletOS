using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
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
	
	private TaskCompletionSource<(bool success, string result)>? _keyboardTcs;
	
	public TopBar()
	{
		InitializeComponent();
		Initialize();
	}

	public void Initialize()
	{
		NumKeyboardGrid.IsVisible = false;
		Statics.RegisterKeyboard(ShowKeyboard);
		
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

	private void LogOutBtn_Click(object? sender, RoutedEventArgs e)
	{
		Statics.YubiCode = string.Empty;
		Statics.YubiSerial = -1;
		Statics.YubiTime = DateTime.MinValue;
		Dispatcher.UIThread.Invoke(() => Statics.ChangeViewModel.Invoke(new MainView()));
	}
	
	#region Keyboard

	public async Task<(bool success, string result)> ShowKeyboard(string originalValue)
	{
		_keyboardTcs = new TaskCompletionSource<(bool, string)>();
		
		KeyboardValueBox.Text = originalValue;
		NumKeyboardGrid.IsVisible = true;
		
		return await _keyboardTcs.Task;
	}
	
	private void CloseKeyboard(object? sender, PointerReleasedEventArgs e)
	{
		NumKeyboardGrid.IsVisible = false;
		_keyboardTcs?.TrySetResult((false, ""));
	}

	private void KeyboardSaveBtn_Click(object? sender, RoutedEventArgs e)
	{
		NumKeyboardGrid.IsVisible = false;
		string text = KeyboardValueBox.Text!;
		
		// Remove trailing comma
		if (text.Last() == ',')
			text = text.Substring(0, text.Length - 1);
		
		_keyboardTcs?.TrySetResult((true, text));
	}

	private void EnterKeyboardValue(string value)
	{
		if (KeyboardValueBox.Text == "0" && value != ",")
			KeyboardValueBox.Text = value;
		else
			KeyboardValueBox.Text += value;
	}

	private void KeyboardNineBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("9");

	private void KeyboardEightBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("8");

	private void KeyboardSevenBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("7");

	private void KeyboardSixBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("6");

	private void KeyboardFiveBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("5");

	private void KeyboardFourBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("4");

	private void KeyboardThreeBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("3");

	private void KeyboardTwoBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("2");

	private void KeyboardOneBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("1");

	private void KeyboardZeroBtn_Click(object? sender, RoutedEventArgs e) => EnterKeyboardValue("0");

	private void KeyboardCommaBtn_Click(object? sender, RoutedEventArgs e)
	{
		if (KeyboardValueBox.Text == null || KeyboardValueBox.Text.Contains(','))
			return;
		
		EnterKeyboardValue(",");
	}

	private void KeyboardDeleteBtn_Click(object? sender, RoutedEventArgs e)
	{
		if (KeyboardValueBox.Text == null)
			return;

		if (KeyboardValueBox.Text.Length <= 1)
		{
			KeyboardValueBox.Text = "0";
			return;
		}
				
		KeyboardValueBox.Text = KeyboardValueBox.Text.Substring(0, KeyboardValueBox.Text.Length - 1);
	}
	
	#endregion
}