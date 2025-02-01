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
		_cancelTokenSource.Cancel();
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingArea.IsVisible = false;
		});
	}

	private void KeyPluggedIn(object? sender, YubiKeyDeviceEventArgs e)
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingName.Text = "Detected key";
			LoadingArea.IsVisible = true;
		});
		Thread.Sleep(50);
		Task.Run(GetKey, _cancelTokenSource.Token);
	}

	private void GetKey()
	{
		IYubiKeyDevice? device = YubiKeyDevice.FindAll().FirstOrDefault();
		if (device == null)
			return;

		using OathSession session = new OathSession(device);
		
		foreach (Credential credential in session.GetCredentials())
		{
			if (credential.Issuer != "AutoTF")
				return;
			
			Statics.YubiCode = session.CalculateCredential(credential).Value!;
			Statics.YubiSerial = device.SerialNumber!.Value;
			Statics.YubiTime = DateTime.UtcNow;
			ChangeScreen();
		}
	}

	private void ChangeScreen()
	{
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = new TrainSelectionScreen();
		}
		_listener.Dispose();
	}
}