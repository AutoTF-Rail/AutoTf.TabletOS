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
#if RELEASE
		RunBridgeScan(true);
#endif
	}

	private void LoadInternetTrains()
	{
		if (Statics.NetworkManager.IsInternetAvailable())
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

	private async void RunBridgeScan(bool isFirst)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo()
		{
			FileName = "btmgmt",
			Arguments = "find",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		Process process = new Process()
		{
			StartInfo = processStartInfo
		};

		process.Start();
		Thread.Sleep(1500);

		StreamReader outputReader = process.StandardOutput;

		List<string> trains = new List<string>();
		
		string? line;
		while ((line = await outputReader.ReadLineAsync()) != null)
		{
			if (line.Contains("name") && line.Contains("CentralBridge-"))
			{
				if(isFirst)
					AddBridge(line.Replace("name ", ""));
				else
					trains.Add(line.Replace("name ", ""));
			}
		}

		if (isFirst)
		{
			_nearbyTrains = new ObservableCollection<TrainAd>(trains.Select(x => new TrainAd
			{
				TrainName = x
			}));
		}

		Dispatcher.UIThread.Invoke(() =>
		{
			if (NearbyLoadingArea.IsVisible)
				NearbyLoadingArea.IsVisible = false;
		});
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

	private void RescanButton_OnClick(object? sender, RoutedEventArgs e)
	{
		RunBridgeScan(false);
	}

	private async void TrainNearby_Click(object? sender, RoutedEventArgs e)
	{
		LoadingArea.IsVisible = true;
		LoadingName.Text = "Trying to connect to train...";
		
		Button button = (Button)sender!;
		TrainAd trainAd = (TrainAd)button.DataContext!;

		if (!TryConnectToNetwork(trainAd.TrainName))
		{
			LoadingName.Text = "Could not find train network...\nRetrying...";

			if (!TryConnectToNetwork(trainAd.TrainName, false))
			{
				ErrorBox.Text = "Could not connect to train. Please move closer and retry.";
				LoadingArea.IsVisible = false;
				return;
			}
		}

		Statics.Connection = ConnectionType.Train;

		LoadingName.Text = "Established connection.\nInitializing...";
		
		try
		{
			string url = "192.168.1.1/information/login?macAddr=" + ExecuteCommand("cat /sys/class/net/wlan0/address") + "&serialNumber=" + Statics.YubiSerial + "&timestamp=" + Statics.YubiTime;

			using HttpClient client = new HttpClient();
			
			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			
			response.EnsureSuccessStatusCode();

			LoadingName.Text = "Logged in...";
			// TODO: Send hello message
		}
		catch (Exception ex)
		{
			ErrorBox.Text = "Could not login. " + ex.Message;
			LoadingArea.IsVisible = false;
		}
	}

	private bool TryConnectToNetwork(string name, bool isFirstTry = true)
	{
		string commandResult = ExecuteCommand(
			$"nmcli dev wifi connect \"{name}\" password \"CentralBridgePW\" hidden yes ifname wlan0");

		if (isFirstTry && commandResult.Contains("No network with SSID"))
			return false;

		if (commandResult.Contains("successfully activated with"))
			return true;
		
		return false;
	}
	
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
		process.WaitForExit();
		return result;
	}
}