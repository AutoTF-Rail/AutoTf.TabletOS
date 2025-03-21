using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
// ReSharper disable UnusedParameter.Local

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainControlView : UserControl
{
	private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;
	private readonly ITrainControlService _trainControl = Statics.TrainControlService;
	private readonly ITrainCameraService _trainCameraService = Statics.TrainCameraService;
	private readonly NetworkService _networkService = Statics.NetworkService;
	private readonly Logger _logger = Statics.Logger;

	private SolidColorBrush _nonePressedEcBtnBackground = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255));
	private Button? _ecCurrentlyPressed;

	private EasyControlView? _easyControlView;

	// TODO: Sync with train on startup
	private Side _currentDirection = Side.Front;
	private Side _currentCamera = Side.Front;
	
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
		if (cameraIndex != (int)_currentCamera)
			return;
		
		Dispatcher.UIThread.Invoke(() =>
		{
			if (_easyControlView != null)
				_easyControlView.CameraViewBig.Source = bitmap;
			
			PreviewImage.Source = bitmap;
		});
	}

	private async Task LoadTrainData()
	{
		string? evuName = await _trainInfo.GetEvuName();
		string? trainId = await _trainInfo.GetTrainId();
		string? trainName = await _trainInfo.GetTrainName();
		string? trainVersion = await _trainInfo.GetVersion();
		
		if (evuName == null || trainId == null || trainName == null || trainVersion == null)
		{
			AddNotification("Could not get train information data. Please view the logs for more information.", Colors.Red);
			return;
		}
		
		await Dispatcher.UIThread.InvokeAsync(() => EvuNameBox.Text = evuName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainIdBox.Text = trainId);
		await Dispatcher.UIThread.InvokeAsync(() => TrainNameBox.Text = trainName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainVersion.Text = trainVersion);
		
		// TODO: Convert this to an actual endpoint that tells you if controls are available
		if (await _trainControl.GetLeverCount() != 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => ControlsUnavailableSection.IsVisible = false);
			await Dispatcher.UIThread.InvokeAsync(() => EmergencyStopButton.IsEnabled = true);
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
				// TODO: Sync direction
				PreviousCamButton.IsVisible = false;
				CamDirectionText.Text = (_currentDirection == _currentCamera) ? "[Front Cam]" : "[Back Cam]";

				// TODO: Sync
				AiDriverStatus.Text = "Disabled";
				AiDriverStatus.Foreground = Brushes.Yellow;

				string[] splashes = File.ReadAllLines("CopiedAssets/AiSplash");
				
				AiNextStop.Text = splashes[new Random().Next(splashes.Length)];

				AiDriverStartStopButton.Content = "Start";
			});

			await LoadTrainData();
		
			await _trainCameraService.StartListeningForCameras();

			await Dispatcher.UIThread.InvokeAsync(() => LoadingArea.IsVisible = false);
		}
		catch (Exception e)
		{
			_logger.Log("Error during control init.");
			_logger.Log(e.ToString());
			// AddNotification("Disconnected: Could not initialize controls.", Colors.Red);
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
			
			// TODO: Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
			// Stop streams
			// Disconnect from wifi
			// Change screen

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
			AddNotification("Could not change into train selection screen. Please restart the device.", Colors.Red);
		}
	}

	private async Task ChangeDirection()
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = "Changing direction.";
			LoadingArea.IsVisible = true;
		});
		
		// TODO: Can't change direction if train is actively moving. In the future just disable the button.
		if (false /*trainIsMoving*/)
		{
			AddNotification("Cannot change direction while train is moving.",
				Colors.Yellow);
			return;
		}
		
#if DEBUG
		await Task.Delay(750);
#endif
		// TODO: Notify train of side change and wait for completion
		
		if (_currentDirection == Side.Front)
			_currentDirection = Side.Back;
		else
			_currentDirection = Side.Front;

		switch (_currentDirection)
		{
			// If new direction is back, and current cam is front: Change cam
			case Side.Back when _currentCamera == Side.Front:
			// If new direction is front, and current cam is back: Change cam
			case Side.Front when _currentCamera == Side.Back:
				ChangeCamera();
				break;
		}
		
		Dispatcher.UIThread.Invoke(() => LoadingArea.IsVisible = false);
	}

	private void ChangeCamera()
	{
		_currentCamera = _currentCamera == Side.Front ? Side.Back : Side.Front;

		bool canShowPreviousBtn = _currentCamera == Side.Back;
		bool canShowNextBtn = _currentCamera == Side.Front;
		
		PreviousCamButton.IsVisible = canShowPreviousBtn;
		NextCamButton.IsVisible = canShowNextBtn;
		
#if DEBUG
		_trainCameraService.StartListeningForCameras();
#endif
		
		Dispatcher.UIThread.Invoke(() => CamDirectionText.Text = (_currentDirection == _currentCamera) ? "[Front Cam]" : "[Back Cam]");
	}


	private void AddNotification(string text, Color color)
	{
		Dispatcher.UIThread.Invoke(() => Statics.Notifications.Add(new Notification(text, color)));
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
	
	private async void TrainInfo_Click(object? sender, RoutedEventArgs e)
	{
		TrainInfoView infoView = new TrainInfoView(_trainInfo);
		if(await infoView.Show(RootGrid) == 1)
			ChangeToSelectionScreen();
	}
	
	private async void ChangeDirection_Click(object? sender, RoutedEventArgs e)
	{
		try
		{
			await ChangeDirection();
		}
		catch (Exception ex)
		{
			_logger.Log("An exception occured while changing the direction:");
			_logger.Log(ex.ToString());
		}
	}
	
	private void EmergencyStopButton_OnClick(object? sender, RoutedEventArgs e)
	{
		_logger.Log("Emergency brake has been invoked.");
		_trainControl.EmergencyBrake();
	}

	private void SpeedLimit_Click(object? sender, RoutedEventArgs e)
	{
		// TODO: Artificial speed limiter that is done by software too?
	}
	
	private void ChangeCamera_Click(object? sender, RoutedEventArgs e)
	{
		ChangeCamera();
	}
	
	#endregion
	
	#region EasyControl

	private void EasyControl_Click_100(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		Ec100Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = Ec100Btn;
		_trainControl.EasyControl(100);
	}

	private void EasyControl_Click_75(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		Ec75Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = Ec75Btn;
		_trainControl.EasyControl(75);
	}

	private void EasyControl_Click_50(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		Ec50Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = Ec50Btn;
		_trainControl.EasyControl(50);
	}

	private void EasyControl_Click_25(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		Ec25Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = Ec25Btn;
		_trainControl.EasyControl(25);
	}

	private void EasyControl_Click_0(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		Ec0Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = Ec0Btn;
		_trainControl.EasyControl(0);
	}

	private void EasyControl_Click_M_25(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		EcM25Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = EcM25Btn;
		_trainControl.EasyControl(-25);
	}

	private void EasyControl_Click_M_50(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		EcM50Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = EcM50Btn;
		_trainControl.EasyControl(-50);
	}

	private void EasyControl_Click_M_75(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		EcM75Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = EcM75Btn;
		_trainControl.EasyControl(-75);
	}

	private void EasyControl_Click_M_100(object? sender, RoutedEventArgs e)
	{
		if(_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;
		
		EcM100Btn.Background = Brushes.Gray;
		_ecCurrentlyPressed = EcM100Btn;
		_trainControl.EasyControl(-100);
	}

	#endregion
}