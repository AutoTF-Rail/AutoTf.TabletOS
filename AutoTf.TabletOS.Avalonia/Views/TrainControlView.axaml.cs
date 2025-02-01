using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Timer = System.Timers.Timer;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainControlView : UserControl
{
	private readonly IDataManager _dataManager = Statics.DataManager;
	private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;
	private readonly ITrainControlService _trainControl = Statics.TrainControlService;
	
	private Timer? _saveTimer = new Timer(600);

	private double _combinedThrottlePosition;

	public TrainControlView()
	{
		InitializeComponent();

		Task.Run(Initialize);
		Task.Run(InitializeStream);
	}
	
	private async Task InitializeStream()
	{
	    try
	    {
	        string url = "http://192.168.1.1/camera/startStream";
	        using (HttpClient client = new HttpClient())
	        {
	            client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

	            HttpResponseMessage response = await client.GetAsync(url);

	            if (!response.IsSuccessStatusCode)
	            {
	                Console.WriteLine("Failed to start the stream on the server.");
	                return;
	            }
	        }

	        UdpClient udpClient = new UdpClient(5001);
	        
	        MemoryStream ms = new MemoryStream();

	        Console.WriteLine("Waiting for frames from the server...");

	        while (true)
	        {
	            UdpReceiveResult result = await udpClient.ReceiveAsync();

	            if (result.Buffer.Length > 0)
	            {
	                ms.Write(result.Buffer, 0, result.Buffer.Length);

	                ms.Seek(0, SeekOrigin.Begin);

	                await Task.Run(() =>
	                {
	                    try
	                    {
	                        using (Bitmap bitmap = new Bitmap(ms))
	                        {
		                        Bitmap bitmapLocal = bitmap;
	                            Dispatcher.UIThread.Invoke(() =>
	                            {
	                                PreviewImage.Source = bitmapLocal;
	                                bitmapLocal.Dispose();
	                            });
	                        }
	                    }
	                    catch (Exception ex)
	                    {
	                        Console.WriteLine("Error processing frame: " + ex.Message);
	                    }
	                });

	                ms.SetLength(0);
	            }
	        }
	    }
	    catch (Exception ex)
	    {
	        Console.WriteLine("Error while receiving the stream:");
	        Console.WriteLine(ex.Message);
	    }
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
		string error = process.StandardError.ReadToEnd();
		
		process.WaitForExit();
		
		if (result == "")
			return error;
		
		return result;
	}

	private async void Initialize()
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = "Loading data...";
			LoadingArea.IsVisible = true;
		});

		await LoadLastConnected();
		await LoadTrainData();
		await LoadControlData();

		await Dispatcher.UIThread.InvokeAsync(() => LoadingArea.IsVisible = false);
	}

	private async Task LoadControlData()
	{
		if (await _trainControl.GetLeverCount() != 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => ControlsUnavailableSection.IsVisible = false);
			await Dispatcher.UIThread.InvokeAsync(() => EmergencyStopButton.IsEnabled = true);
		}
		_combinedThrottlePosition = await _trainControl.GetLeverPosition(0);
		
		await Dispatcher.UIThread.InvokeAsync(() => CombinedThrottlePercentage.Text = _combinedThrottlePosition.ToString());
	}

	private async Task LoadTrainData()
	{
		string? evuName = await _trainInfo.GetEvuName();
		string? trainId = await _trainInfo.GetTrainId();
		string? trainName = await _trainInfo.GetTrainName();
		string? lastTrainSync = await _trainInfo.GetLastSync();
		string? trainVersion = await _trainInfo.GetVersion();
		await UpdateSaveTimer();
		
		if (evuName == null || trainId == null || trainName == null || lastTrainSync == null || trainVersion == null)
		{
			// TODO: Disconnect
			// TODO: Log
			return;
		}

		int dayDiff = (DateTime.Parse(lastTrainSync) - DateTime.Now).Days * -1;

		IImmutableSolidColorBrush brush = ConvertDayIntoBrush(dayDiff);
		await Dispatcher.UIThread.InvokeAsync(() => NextTrainConnectionDay.Foreground = brush);

		if (dayDiff >= 30)
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextTrainConnectionDay.Text = $"Long due.");
		else
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextTrainConnectionDay.Text = (30 - dayDiff) + " Days");
		
		await Dispatcher.UIThread.InvokeAsync(() => EvuNameBox.Text = evuName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainIdBox.Text = trainId);
		await Dispatcher.UIThread.InvokeAsync(() => TrainNameBox.Text = trainName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainVersion.Text = trainVersion);
	}

	private async Task UpdateSaveTimer()
	{
		DateTime? nextSave = await _trainInfo.GetNextSave();
		if (nextSave == null)
		{
			await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = "Unknown");
			return;
		}
		int nextSaveInMs = (nextSave.Value.Add(TimeSpan.FromSeconds(2)) - DateTime.Now).Milliseconds;
		if (nextSaveInMs <= 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = "Past Due");
			return;
		}
		_saveTimer?.Dispose();
		_saveTimer = new Timer(nextSaveInMs);
		_saveTimer.Elapsed += (_, _) => _ = UpdateSaveTimer();
		
		_saveTimer.Start();

		string date = nextSave.Value.ToString("HH:mm:ss");
		await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = date);
	}

	private async Task LoadLastConnected()
	{
		int dayDiff = (_dataManager.GetLastSynced() - DateTime.Now).Days * -1;
		
		IImmutableSolidColorBrush brush = ConvertDayIntoBrush(dayDiff);
		await Dispatcher.UIThread.InvokeAsync(() => NextConnectionDay.Foreground = brush);

		if (dayDiff >= 30)
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextConnectionDay.Text = $"Please connect to the internet as soon as possible.");
		else
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextConnectionDay.Text = $"Please connect to the internet in {30 - dayDiff} days.");
	}

	private IImmutableSolidColorBrush ConvertDayIntoBrush(int dayDif)
	{
		return dayDif switch
		{
			< 10 => Brushes.Green,
			< 20 => Brushes.Yellow,
			_ => Brushes.Red
		};
	}

	private void ChangeTrain_Click(object? sender, RoutedEventArgs e)
	{
		// Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
		// Stop streams
		// Disconnect from wifi
		// Change screen

		ExecuteCommand("nmcli connection delete id CentralBridge-" + Statics.TrainConnectionId);
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = new TrainSelectionScreen();
		}
	}
	
	private async void ShutdownTrain_Click(object? sender, RoutedEventArgs e)
	{
		await _trainInfo.PostShutdown();
	}

	private async void RestartTrain_Click(object? sender, RoutedEventArgs e)
	{
		await _trainInfo.PostRestart();
	}

	private async void UpdateTrain_Click(object? sender, RoutedEventArgs e)
	{
		await _trainInfo.PostUpdate();
	}

	private async void CombinedThrottleUp_Click(object? sender, RoutedEventArgs e)
	{
		if (_combinedThrottlePosition >= 100)
			return;

		_combinedThrottlePosition += 10;
		await _trainControl.SetLever(0, _combinedThrottlePosition);
		await Dispatcher.UIThread.InvokeAsync(() => CombinedThrottlePercentage.Text = _combinedThrottlePosition.ToString());
	}

	private async void CombinedThrottleDown_Click(object? sender, RoutedEventArgs e)
	{
		if (_combinedThrottlePosition <= -100)
			return;

		_combinedThrottlePosition -= 10;
		await _trainControl.SetLever(0, _combinedThrottlePosition);
		await Dispatcher.UIThread.InvokeAsync(() => CombinedThrottlePercentage.Text = _combinedThrottlePosition.ToString());
	}
}