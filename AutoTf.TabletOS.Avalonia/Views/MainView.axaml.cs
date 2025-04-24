using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Yubico.YubiKey;
using Yubico.YubiKey.Oath;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainView : UserControl
{
	private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
	private YubiKeyDeviceListener _listener;
	private bool _isHandlingKey;
	
	public MainView()
	{
		InitializeComponent();
		
		LoadingArea.IsVisible = false;
		
		_listener = YubiKeyDeviceListener.Instance;
		_listener.Arrived += KeyPluggedIn;
		_listener.Removed += KeyRemoved;

		TryDetectAlreadyPluggedIn();
		
#if RELEASE
		SkipButton.IsVisible = false;
#endif
	}

	private void TryDetectAlreadyPluggedIn()
	{
		if (_isHandlingKey)
			return;
		_isHandlingKey = true;
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingName.Text = "Checking for key.";
			LoadingArea.IsVisible = true;
		});
		Thread.Sleep(25);
		IYubiKeyDevice? key = YubiKeyDevice.FindAll().FirstOrDefault();
		if (key == null)
		{
			Dispatcher.UIThread.Invoke(() =>
			{;
				LoadingArea.IsVisible = false;
			});
			return;
		}
		Task.Run(() => GetKey(key), _cancelTokenSource.Token);
	}

	private void KeyRemoved(object? sender, YubiKeyDeviceEventArgs e)
	{
		_isHandlingKey = false;
		_cancelTokenSource.Cancel();
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingArea.IsVisible = false;
		});
	}

	private void KeyPluggedIn(object? sender, YubiKeyDeviceEventArgs e)
	{
		if (_isHandlingKey)
			return;
		_isHandlingKey = true;
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingName.Text = "Detected key.";
			LoadingArea.IsVisible = true;
		});
		Thread.Sleep(25);
		Task.Run(() => GetKey(e.Device), _cancelTokenSource.Token);
	}

	private void GetKey(IYubiKeyDevice device)
	{
		using OathSession session = new OathSession(device);

		Credential? atfCred = session.GetCredentials().FirstOrDefault(c => c.Issuer == "AutoTF");
		
		if (atfCred is not null)
		{
			Statics.YubiCode = session.CalculateCredential(atfCred).Value!;
			Statics.YubiSerial = device.SerialNumber!.Value;
			Statics.YubiTime = DateTime.UtcNow;
			ChangeScreen();
		}
		else
			Statics.Notifications.Add(new Notification("Could not find AutoTF Credential on yubikey.", Colors.Yellow));

		Dispatcher.UIThread.Invoke(() => LoadingArea.IsVisible = false);
	}

	private void ChangeScreen()
	{
		Statics.ChangeViewModel.Invoke(new TrainSelectionScreen());
		_listener.Dispose();
	}

	private void SkipButton_Click(object? sender, RoutedEventArgs e)
	{
	#if DEBUG
		Statics.YubiCode = "123";
		Statics.YubiSerial = 1;
		Statics.YubiTime = DateTime.UtcNow;
		ChangeScreen();
	#endif
	}
}