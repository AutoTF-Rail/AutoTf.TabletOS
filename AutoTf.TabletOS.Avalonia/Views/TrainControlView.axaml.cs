using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
// ReSharper disable UnusedParameter.Local

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainControlView : UserControl
{
	private readonly SolidColorBrush _nonePressedEcBtnBackground = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255));
	private Button? _ecCurrentlyPressed;
	
	private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;
	private readonly ITrainControlService _trainControl = Statics.TrainControlService;
	private readonly ITrainCameraService _trainCameraService = Statics.TrainCameraService;
	private readonly INetworkService _networkService = Statics.NetworkService;
	private readonly Logger _logger = Statics.Logger;
	
	private EasyControlView? _easyControlView;

	// TODO: Sync with train on startup
	private Side _currentDirection = Side.Front;
	private Side _currentCamera = Side.Front;

	public TrainControlView()
	{
		try
		{
			InitializeComponent();

			_ = Task.Run(async () => await Initialize());
		}
		catch (Exception e)
		{
			_logger.Log("Error while initializing view:");
			_logger.Log(e.ToString());
		}
	}

	#region Initialization
	private async Task Initialize()
	{
		try
		{
			await InvokeLoadingScreen(true);

			_trainCameraService.NewFrameReceived += NewFrameReceived;
			
			Task aicTask = LoadAicData();
			Task trainDataTask = LoadTrainData();
			Task trainCamTask = _trainCameraService.StartListeningForCameras();

			await Task.WhenAll(aicTask, trainDataTask, trainCamTask);
		}
		catch (Exception e)
		{
			_logger.Log("Error during control init.");
			_logger.Log(e.ToString());
		}
		finally
		{
			await InvokeLoadingScreen(false);
			await UpdateCameraTitle();
		}
	}

	private async Task LoadAicData()
	{
		string[] splashes = await File.ReadAllLinesAsync("CopiedAssets/AiSplash");
		
		AicInformation aicInfo = new AicInformation();
		await aicInfo.UpdateState();
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			// TODO: Sync direction
			PreviousCamButton.IsVisible = false;

			AiDriverStatus.Text = aicInfo.State;
			AiDriverStatus.Foreground = aicInfo.Color;

			AiNextStop.Text = splashes[new Random().Next(splashes.Length)];
		});
	}
	
	private async Task LoadTrainData()
	{
		TrainInformation info = new TrainInformation();
		
		if (!info.InitializedSuccessfully)
		{
			await AddNotification("Could not get train all information data. To resolve this, maybe reconnect..", Colors.Yellow);
		}
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			EvuNameBox.Text = info.EvuName;
			TrainIdBox.Text = info.TrainId;
			TrainNameBox.Text = info.TrainName;
			TrainVersion.Text = info.TrainVersion;
		});
		
		bool ecAvailable = await _trainControl.IsEasyControlAvailable();
		await UpdateControlsAvailability(ecAvailable);
	}
	
	#endregion

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
	
	private async void ChangeToSelectionScreen()
	{
		try
		{
			_logger.Log("Changing to train selection by request.");
			await InvokeLoadingScreen(true, "Disconnecting...");
			
			_trainCameraService.DisconnectStreams();
			
			// TODO: Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
			_networkService.ShutdownConnection();

			Dispatcher.UIThread.Invoke(() => Statics.ChangeViewModel.Invoke(new TrainSelectionScreen()));
		}
		catch (Exception e)
		{
			_logger.Log("Could not change to selection screen:");
			_logger.Log(e.ToString());
			await AddNotification("Could not change into train selection screen. Please restart the device.", Colors.Red);
		}
	}

	private async Task ChangeDirection()
	{
		await InvokeLoadingScreen(true, "Changing direction.");
		
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
			case Side.Back when _currentCamera == Side.Back:
				await UpdateCameraTitle();
				break;
		}

		await InvokeLoadingScreen(false);
	}

	private async void ChangeCamera()
	{
		_currentCamera = _currentCamera == Side.Front ? Side.Back : Side.Front;

		bool canShowPreviousBtn = _currentCamera == Side.Back;
		bool canShowNextBtn = _currentCamera == Side.Front;
	
		PreviousCamButton.IsVisible = canShowPreviousBtn;
		NextCamButton.IsVisible = canShowNextBtn;
	
#if DEBUG
		await _trainCameraService.StartListeningForCameras();
#endif
	
		await UpdateCameraTitle();
	}

#region dispatchers

	private async Task UpdateCameraTitle()
	{
		await Dispatcher.UIThread.InvokeAsync(() => CamDirectionText.Text = _currentDirection == _currentCamera ? "[Front Cam]" : "[Back Cam]");
	}
	
	private async Task AddNotification(string text, Color color)
	{
		await Dispatcher.UIThread.InvokeAsync(() => Statics.Notifications.Add(new Notification(text, color)));
	}
	
	private async Task UpdateControlsAvailability(bool available)
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			ControlsUnavailableSection.IsVisible = !available;
			EmergencyStopButton.IsEnabled = available;
		});
	}
	
	private async Task InvokeLoadingScreen(bool visible, string additionalText = "Loading data...")
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = additionalText;
			LoadingArea.IsVisible = visible;
		});
	}
	
#endregion
	
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

	private async void AicControl_Click(object? sender, RoutedEventArgs e)
	{
		AicControlView aic = new AicControlView();
		await aic.Show(RootGrid);
	}
	
	#endregion
	
	#region EasyControl
	
	private async void HandleEcButtonClick(Button button, int value)
	{
		Button? previousButton = _ecCurrentlyPressed;
		if (_ecCurrentlyPressed != null)
			_ecCurrentlyPressed.Background = _nonePressedEcBtnBackground;

		button.Background = Brushes.Gray;
		_ecCurrentlyPressed = button;

		if (await _trainControl.EasyControl(value)) 
			return;
		
		await AddNotification("Something went wrong when setting the EasyControl value. Please restart the train.", Colors.Red);
		_ecCurrentlyPressed = previousButton;
			
		if (previousButton != null) 
			previousButton.Background = Brushes.Gray;
			
		button.Background = _nonePressedEcBtnBackground;
	}
	
	private void EasyControl_Click_100(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, 100);

	private void EasyControl_Click_75(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, 75);

	private void EasyControl_Click_50(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, 50);

	private void EasyControl_Click_25(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, 25);

	private void EasyControl_Click_0(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, 0);

	private void EasyControl_Click_M_25(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, -25);

	private void EasyControl_Click_M_50(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, -50);

	private void EasyControl_Click_M_75(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, -75);

	private void EasyControl_Click_M_100(object? sender, RoutedEventArgs e) => HandleEcButtonClick(Ec100Btn, -100);

	#endregion
}