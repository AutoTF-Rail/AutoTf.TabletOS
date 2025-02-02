using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainSelectionScreen : UserControl
{
	private ObservableCollection<TrainAd> _nearbyTrains = new ObservableCollection<TrainAd>();
	private readonly NetworkManager _networkManager = Statics.NetworkManager;
	private readonly Logger _logger = Statics.Logger;
	
	public TrainSelectionScreen()
	{
		InitializeComponent();
		NearbyTrains.ItemsSource = _nearbyTrains;
		#if DEBUG
		_nearbyTrains.Add(new TrainAd()
		{
			TrainName = "Meow"
		});
		NearbyLoadingArea.IsVisible = false;
		LoadingArea.IsVisible = false;
		#endif
		Task.Run(Initialize);
	}

	private void Initialize()
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingArea.IsVisible = false;
		});
		
		LoadInternetTrains();
		LoadNearbyTrains();
	}

	private void LoadNearbyTrains()
	{
#if DEBUG
		return;
		_logger.Log("Not running bluetooth scan due to not being in RELEASE");
#endif
		RunBridgeScan();
	}

	private void LoadInternetTrains()
	{
		if (NetworkManager.IsInternetAvailable())
		{
			// Show text that trains aren't available due to no internet
		}
	}

	private void NearbyTrains_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		NearbyTrains.SelectedItem = null;
	}

	private void OtherTrains_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		OtherTrains.SelectedItem = null;
	}

	private async Task RunBridgeScan()
	{
		try
		{
			_logger.Log("Scanning for trains.");
			_nearbyTrains.Clear();
			ProcessStartInfo processStartInfo = new ProcessStartInfo()
			{
				FileName = "timeout",
				Arguments = "6s btmgmt find",
				RedirectStandardOutput = true,
				RedirectStandardError = false,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process process = new Process()
			{
				StartInfo = processStartInfo
			};

			process.Start();
			process.WaitForExit(6000);

			StreamReader outputReader = process.StandardOutput;

			while (await outputReader.ReadLineAsync() is { } line)
			{
				if (line.Contains("name") && line.Contains("CentralBridge-"))
				{
					if (_nearbyTrains.Any(x => x.TrainName == line.Replace("name ", "")))
						continue;
					AddBridge(line.Replace("name ", ""));
				}
			}

			Dispatcher.UIThread.Invoke(() =>
			{
				if (NearbyLoadingArea.IsVisible)
					NearbyLoadingArea.IsVisible = false;
			});
			
			_logger.Log("Done scanning for nearby devices");
		}
		catch (Exception e)
		{
			_logger.Log("------------------Scan error:");
			_logger.Log(e.Message);
			Dispatcher.UIThread.Invoke(() =>
			{
				if (NearbyLoadingArea.IsVisible)
					NearbyLoadingArea.IsVisible = false;
			});
		}
	}

	private void AddBridge(string name)
	{
		_logger.Log("Adding train: " + name);
		_nearbyTrains.Add(new TrainAd()
		{
			TrainName = name
		});
		
		Dispatcher.UIThread.Invoke(() =>
		{
			if (NearbyLoadingArea.IsVisible)
				NearbyLoadingArea.IsVisible = false;
		});
		Thread.Sleep(25);
	}

	private async void RescanButton_OnClick(object? sender, RoutedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			RescanButton.IsVisible = false;
			NearbyLoadingArea.IsVisible = true;
		});
		await Task.Delay(50);
		
		await RunBridgeScan();
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			RescanButton.IsVisible = true;
			NearbyLoadingArea.IsVisible = false;
		});
	}

	private async void TrainNearby_Click(object? sender, RoutedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingArea.IsVisible = true;
			LoadingName.Text = "Trying to connect to train...";
		});
		await Task.Delay(250);
		
		TrainAd trainAd = (TrainAd)((Button)sender!).DataContext!;

		Statics.TrainConnectionId = Statics.GenerateRandomString();

		// TODO: set this: export YUBICO_LOG_LEVEL=ERROR
		string? connOutput = _networkManager.EstablishConnection(trainAd.TrainName, true);

		if (connOutput != null)
		{
			_logger.Log("Connection error:");
			_logger.Log(connOutput);
			Dispatcher.UIThread.Invoke(() =>
			{
				ErrorBox.Text = "Could not connect to train. Please move closer and retry.";
				LoadingArea.IsVisible = false;
			});
			return;
		}

		await TryLogin();
	}

	private async Task TryLogin()
	{
		Dispatcher.UIThread.Invoke(() => LoadingName.Text = "Logging in...");
		
		try
		{
			string url = "http://192.168.1.1/information/login?macAddr=" + CommandExecuter.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd() + "&serialNumber=" + Statics.YubiSerial + "&code=" + Statics.YubiCode + "&timestamp=" + Statics.YubiTime.ToString("yyyy-MM-ddTHH:mm:ss");

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage loginResponse = await loginClient.PostAsync(url, new StringContent(""));
			
			loginResponse.EnsureSuccessStatusCode();
			
			Dispatcher.UIThread.Invoke(() =>
			{
				LoadingName.Text = "Loading panel...";
				if (DataContext is MainWindowViewModel viewModel)
				{
					viewModel.ActiveView = new TrainControlView();
				}
			});
		}
		catch (Exception ex)
		{
			_networkManager.ShutdownConnection();
			Dispatcher.UIThread.Invoke(() =>
			{
				ErrorBox.Text = "Could not login. " + ex.Message;
				LoadingArea.IsVisible = false;
			});
		}
	}
}