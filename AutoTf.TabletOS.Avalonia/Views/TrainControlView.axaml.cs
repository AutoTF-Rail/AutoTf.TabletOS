using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.Logging;
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
	private readonly NetworkManager _networkManager = Statics.NetworkManager;
	private readonly Logger _logger = Statics.Logger;
	
	private Timer? _saveTimer = new Timer(600);
	
	private Bitmap? _currentBitmap;
	
	private double _combinedThrottlePosition;
	private bool _canListenForStream = true;

	public TrainControlView()
	{
		try
		{
			InitializeComponent();

			Statics.Shutdown += () => _trainInfo.PostStopStream();

			Task.Run(Initialize);
			Task.Run(InitializeStream);
		}
		catch (Exception e)
		{
			_logger.Log("Error while initializing view:");
			_logger.Log(e.Message);
		}
	}
	
	private async Task InitializeStream()
	{
		try
		{
			_logger.Log("Starting stream");
			if (!await _trainInfo.PostStartStream())
			{
				_logger.Log("Could not start stream");
				return;
				// TODO: Show error that it failed to start the stream
			}

			_logger.Log("Listening for images.");
			
			int udpPort = 12345;
			UdpClient udpClient = new UdpClient(udpPort);

			while (_canListenForStream)
			{
				UdpReceiveResult result = await udpClient.ReceiveAsync();
				byte[] frameData = result.Buffer;

				if (frameData.Length == 0)
				{
					_logger.Log("Received empty frame data.");
					Thread.Sleep(25);
					continue;
				}
				
				using (MemoryStream ms = new MemoryStream(frameData))
				{
					if (_currentBitmap != null)
						_currentBitmap.Dispose();
					
					_currentBitmap = new Bitmap(ms);
					// ReSharper disable once AccessToDisposedClosure
					Dispatcher.UIThread.Invoke(() => { PreviewImage.Source = _currentBitmap!; });
				}
			}
			udpClient.Dispose();
		}
		catch (Exception ex)
		{
			_logger.Log("Error while getting UDP stream:");
			_logger.Log(ex.ToString());
		}
	}

	private async void Initialize()
	{
		try
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
		catch (Exception e)
		{
			_logger.Log("Error during control init.");
			_logger.Log(e.Message);
			ChangeTrain_Click(null, null);
		}
	}

	private async Task LoadControlData()
	{
		if (await _trainControl.GetLeverCount() != 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => ControlsUnavailableSection.IsVisible = false);
			await Dispatcher.UIThread.InvokeAsync(() => EmergencyStopButton.IsEnabled = true);
		}
		_combinedThrottlePosition = await _trainControl.GetLeverPosition(0);
		
		await Dispatcher.UIThread.InvokeAsync(() => CombinedThrottlePercentage.Text = _combinedThrottlePosition.ToString(CultureInfo.InvariantCulture));
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
		_saveTimer?.Dispose();
		DateTime? nextSave = await _trainInfo.GetNextSave();
		if (nextSave == null)
		{
			ChangeTrain_Click(null, null);
			await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = "Unknown");
			return;
		}
		int nextSaveInMs = (nextSave.Value.Add(TimeSpan.FromSeconds(2)) - DateTime.Now).Milliseconds;
		if (nextSaveInMs <= 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = "Past Due");
			return;
		}
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

	private async void ChangeTrain_Click(object? sender, RoutedEventArgs e)
	{
		// TODO: Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
		// Stop streams
		// Disconnect from wifi
		// Change screen
		_canListenForStream = false;
		await _trainInfo.PostStopStream();

		_networkManager.ShutdownConnection();

		Dispatcher.UIThread.Invoke(() =>
		{
			if (DataContext is MainWindowViewModel viewModel)
			{
				viewModel.ActiveView = new TrainSelectionScreen();
			}
		});
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