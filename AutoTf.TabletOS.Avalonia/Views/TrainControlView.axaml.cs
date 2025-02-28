using System;
using System.Globalization;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
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
	private readonly ITrainCameraService _trainCameraService = Statics.TrainCameraService;
	private readonly NetworkService _networkService = Statics.NetworkService;
	private readonly Logger _logger = Statics.Logger;

	
	private double _combinedThrottlePosition;

	private EasyControlView? _easyControlView;

	private int _currentCamera = 0;
	
	public TrainControlView()
	{
		try
		{
			InitializeComponent();
			
			_trainCameraService.NewFrameReceived += NewFrameReceived;

			Task.Run(Initialize);
		}
		catch (Exception e)
		{
			_logger.Log("Error while initializing view:");
			_logger.Log(e.Message);
		}
	}

	private void NewFrameReceived(int cameraIndex, Bitmap bitmap)
	{
		if (cameraIndex != _currentCamera)
			return;
		
		Dispatcher.UIThread.Invoke(() =>
		{
			if (_easyControlView != null)
				_easyControlView.CameraViewBig.Source = bitmap;
			
			PreviewImage.Source = bitmap;
		});
	}

	private async Task LoadControlData()
	{
		if (await _trainControl.GetLeverCount() != 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => ControlsUnavailableSection.IsVisible = false);
			await Dispatcher.UIThread.InvokeAsync(() => EmergencyStopButton.IsEnabled = true);
		}
		// TODO: Doesnt this need to be in the upper if too?
		_combinedThrottlePosition = await _trainControl.GetLeverPosition(0);
		
		// await Dispatcher.UIThread.InvokeAsync(() => CombinedThrottlePercentage.Text = _combinedThrottlePosition.ToString(CultureInfo.InvariantCulture));
	}

	private async Task LoadTrainData()
	{
		string? evuName = await _trainInfo.GetEvuName();
		string? trainId = await _trainInfo.GetTrainId();
		string? trainName = await _trainInfo.GetTrainName();
		string? trainVersion = await _trainInfo.GetVersion();
		
		if (evuName == null || trainId == null || trainName == null || trainVersion == null)
		{
			Statics.Notifications.Add(new Notification("Could not get train information data. Please view the logs for more information.", Colors.Red));
			return;
		}
		await Dispatcher.UIThread.InvokeAsync(() => EvuNameBox.Text = evuName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainIdBox.Text = trainId);
		await Dispatcher.UIThread.InvokeAsync(() => TrainNameBox.Text = trainName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainVersion.Text = trainVersion);
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

			await LoadTrainData();
			await LoadControlData();
		
			await _trainCameraService.StartListeningForCameras();

			await Dispatcher.UIThread.InvokeAsync(() => LoadingArea.IsVisible = false);
		}
		catch (Exception e)
		{
			_logger.Log("Error during control init.");
			_logger.Log(e.Message);
			Statics.Notifications.Add(new Notification("Disconnected: Could not initialize controls.", Colors.Red));
			// TODO: Make controls unavailable?
		}
	}
	
	private async void ChangeToSelectionScreen()
	{
		try
		{
			_logger.Log("Changing to train selection by request.");
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				LoadingName.Text = "Disconnecting...";
				LoadingArea.IsVisible = true;
			});
			await Task.Delay(25);
			_trainCameraService.DisconnectStreams();
			_trainCameraService.Dispose();
			
			// TODO: Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
			// Stop streams
			// Disconnect from wifi
			// Change screen
		
			_trainCameraService.DisconnectStreams();

			_networkService.ShutdownConnection();

			Dispatcher.UIThread.Invoke(() =>
			{
				if (DataContext is MainWindowViewModel viewModel)
				{
					viewModel.ActiveView = new TrainSelectionScreen();
				}
			});
		}
		catch (Exception e)
		{
			_logger.Log("Could not change to selection screen:");
			_logger.Log(e.ToString());
			Statics.Notifications.Add(new Notification("Could not change into train selection screen. Please restart the device.", Colors.Red));
		}
	}

	#region UI_Events

	private void ChangeTrain_Click(object? sender, RoutedEventArgs e)
	{
		ChangeToSelectionScreen();
	}

	private async void Control_Click(object? sender, RoutedEventArgs e)
	{
		_logger.Log("Starting easy control.");
		_easyControlView = new EasyControlView();
		await _easyControlView.Show(RootGrid);
		_easyControlView = null;
		_logger.Log("Exited easy control.");
	}
	#endregion

	private async void TrainInfo_Click(object? sender, RoutedEventArgs e)
	{
		TrainInfoView infoView = new TrainInfoView(_trainInfo);
		if(await infoView.Show(RootGrid) == 1)
			ChangeToSelectionScreen();
	}
}