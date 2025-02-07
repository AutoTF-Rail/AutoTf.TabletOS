using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia.Controls;
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

		Credential? atfCred = null;
		
		foreach (Credential credential in session.GetCredentials())
		{
			if (credential.Issuer == "AutoTF")
			{
				atfCred = credential;
				break;
			}
		}

		if (atfCred is not null)
		{
			Statics.YubiCode = session.CalculateCredential(atfCred).Value!;
			Statics.YubiSerial = device.SerialNumber!.Value;
			Statics.YubiTime = DateTime.UtcNow;
			ChangeScreen();
		}

		Dispatcher.UIThread.Invoke(() => LoadingArea.IsVisible = false);
	}

	private void ChangeScreen()
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			if (DataContext is MainWindowViewModel viewModel)
			{
				viewModel.ActiveView = new TrainSelectionScreen();
			}
			_listener.Dispose();
		});
	}
}