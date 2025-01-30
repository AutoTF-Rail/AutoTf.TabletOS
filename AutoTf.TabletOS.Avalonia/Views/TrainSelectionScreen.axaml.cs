using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainSelectionScreen : UserControl
{
	private ObservableCollection<TrainAd> _nearbyTrains = new ObservableCollection<TrainAd>();
	
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
		Console.WriteLine("Not running bluetooth scan due to not being in RELEASE");
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
			Console.WriteLine("Scanning for trains.");
			_nearbyTrains.Clear();
			ProcessStartInfo processStartInfo = new ProcessStartInfo()
			{
				FileName = "timeout",
				Arguments = "3s btmgmt find",
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
			Thread.Sleep(2500);

			StreamReader outputReader = process.StandardOutput;

			List<string> trains = new List<string>();
			
			string? line;
			while ((line = await outputReader.ReadLineAsync()) != null)
			{
				Console.WriteLine("------------" + line);
				if (line.Contains("name") && line.Contains("CentralBridge-"))
				{
					Console.WriteLine("------------" + line);
					AddBridge(line.Replace("name ", ""));
				}
			}

			Dispatcher.UIThread.Invoke(() =>
			{
				if (NearbyLoadingArea.IsVisible)
					NearbyLoadingArea.IsVisible = false;
			});
			
			Console.WriteLine("Done scanning for nearby devices");
		}
		catch (Exception e)
		{
			Console.WriteLine("------------------Scan error:");
			Console.WriteLine(e.Message);
		}
	}

	private void AddBridge(string name)
	{
		Console.WriteLine("Adding train: " + name);
		Dispatcher.UIThread.Invoke(() =>
		{
			_nearbyTrains.Add(new TrainAd()
			{
				TrainName = name
			});
			
			if (NearbyLoadingArea.IsVisible)
				NearbyLoadingArea.IsVisible = false;
		});
	}

	private async void RescanButton_OnClick(object? sender, RoutedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(() => RescanButton.IsVisible = false);
		await RunBridgeScan();
		await Dispatcher.UIThread.InvokeAsync(() => RescanButton.IsVisible = true);
	}

	private async void TrainNearby_Click(object? sender, RoutedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingArea.IsVisible = true;
			LoadingName.Text = "Trying to connect to train...";
		});
		
		
		Button button = (Button)sender!;
		TrainAd trainAd = (TrainAd)button.DataContext!;

		
		string connectionId = Statics.GenerateRandomString();
		Console.WriteLine("Connection ID: " + connectionId);
		ExecuteCommand($"nmcli c add type wifi con-name {connectionId} ifname wlan0 ssid {trainAd.TrainName}");
		ExecuteCommand($"nmcli con modify {connectionId} wifi-sec.key-mgmt wpa-psk");
		ExecuteCommand($"nmcli con modify {connectionId} wifi-sec.psk CentralBridgePW");
		string connOutput = ExecuteCommand($"nmcli con up {connectionId}");

		// TODO: set this: export YUBICO_LOG_LEVEL=ERROR
		
		if (!connOutput.Contains("Connection successfully activated"))
		{
			Console.WriteLine("Connection error:");
			Console.WriteLine(connOutput);
			Dispatcher.UIThread.Invoke(() =>
			{
				ErrorBox.Text = "Could not connect to train. Please move closer and retry.";
				LoadingArea.IsVisible = false;
			});
			return;
		}

		Statics.Connection = ConnectionType.Train;

		Dispatcher.UIThread.Invoke(() => LoadingName.Text = "Established connection.\nInitializing...");
		
		try
		{
			// Key login
			string url = "http://192.168.1.1/information/login?macAddr=" + ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd() + "&serialNumber=" + Statics.YubiSerial + "&code=" + Statics.YubiCode + "&timestamp=" + Statics.YubiTime.ToString("yyyy-MM-ddTHH:mm:ss");

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage loginResponse = await loginClient.PostAsync(url, new StringContent(""));
			
			loginResponse.EnsureSuccessStatusCode();
			
			Dispatcher.UIThread.Invoke(() =>
			{
				if (DataContext is MainWindowViewModel viewModel)
				{
					viewModel.ActiveView = new TrainControlView();
				}
			});
		}
		catch (Exception ex)
		{
			Dispatcher.UIThread.Invoke(() =>
			{
				ErrorBox.Text = "Could not login. " + ex.Message;
				LoadingArea.IsVisible = false;
			});
		}
	}

	// TODO: Dont auto connect to a train in this screen
	
	private string ExecuteCommand(string command)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			}
		};

		process.Start();
		string result = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		
		process.WaitForExit();
		
		if (result == "")
			return error;
		
		return result;
	}
}