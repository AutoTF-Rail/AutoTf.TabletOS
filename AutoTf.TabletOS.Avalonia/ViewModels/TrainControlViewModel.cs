using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.Views;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels;

public class TrainControlViewModel : ReactiveObject
{
    private readonly Logger _logger;
    private readonly IViewRouter _viewRouter;
    private readonly ITrainCameraService _trainCameraService;
    private readonly TrainCameraInformation _trainCamInfo;
    private readonly INetworkService _networkService;

    private Bitmap _currentView = new Bitmap("Assets/LoadingCameraFeedImage.png");
    
    public string CameraText => _trainCamInfo.CurrentDirection == _trainCamInfo.CurrentCamera ? "[Front Cam]" : "[Back Cam]";
    
    public bool PreviousCamBtnVisible => _trainCamInfo.CurrentCamera == Side.Back;
    public bool NextCamBtnVisible => _trainCamInfo.CurrentCamera == Side.Front;
    
    public Bitmap CurrentView
    {
        get => _currentView;
        private set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }
    
    public RelayCommand ChangeCameraCommand { get; }
    public RelayCommand ChangeToTrainSelectionCommand { get; }

    public TrainControlViewModel(Logger logger, IViewRouter viewRouter, ITrainCameraService trainCameraService, TrainCameraInformation trainCamInfo, INetworkService networkService)
    {
        _logger = logger;
        _viewRouter = viewRouter;
        _trainCameraService = trainCameraService;
        _trainCamInfo = trainCamInfo;
        _networkService = networkService;

        ChangeCameraCommand = new RelayCommand(ChangeCamera);
        ChangeToTrainSelectionCommand = new RelayCommand(ChangeToTrainSelection);
        
        _trainCamInfo.WhenAnyPropertyChanged().Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(CameraText));
            this.RaisePropertyChanged(nameof(PreviousCamBtnVisible));
            this.RaisePropertyChanged(nameof(NextCamBtnVisible));
        });
        
#if DEBUG
        _trainCamInfo.WhenAnyPropertyChanged().Subscribe(async void (_) =>
        {
            await _trainCameraService.StartListeningForCameras();
        });
#endif

        Initialize();
    }

    private void ChangeToTrainSelection()
    {
        try
        {
            _logger.Log("Changing to train selection by request.");
            _viewRouter.InvokeLoadingArea(true, "Disconnecting...");
            
            _trainCameraService.DisconnectStreams();
			
            // TODO: Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
            _networkService.ShutdownConnection();

            _viewRouter.NavigateTo<TrainSelectionView>();
        }
        catch (Exception e)
        {
            _logger.Log("Could not change to selection screen:");
            _logger.Log(e.ToString());
            // TOOD: Idk
            Environment.Exit(0);
        }
    }

    private void ChangeCamera()
    {
        _trainCamInfo.CurrentCamera = _trainCamInfo.CurrentCamera  == Side.Front ? Side.Back : Side.Front;
    }

    private async void Initialize()
    {
        try
        {
            // TODO: Sync direction
            
            _viewRouter.InvokeLoadingArea(true, "Loading data...");

            _trainCameraService.NewFrameReceived += NewFrameReceived;
			
            // Oh well... it works
            List<Task> tasks =
            [
                _trainCameraService.StartListeningForCameras()
            ];

            Dictionary<Task, string> taskNames = new Dictionary<Task, string>
            {
                [tasks[0]] = "Camera"
            };
			
            _viewRouter.InvokeLoadingArea(true, $"Loading: {string.Join(", ", taskNames.Values)}");
            while (tasks.Count > 0)
            {
                Task finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);
                taskNames.Remove(finished);

                _viewRouter.InvokeLoadingArea(true, $"Loading: {string.Join(", ", taskNames.Values)}");

                await finished;
            }
        }
        catch (Exception e)
        {
            _logger.Log("Error during control init.");
            _logger.Log(e.ToString());
        }
        finally
        {
            _viewRouter.InvokeLoadingArea(false);
        }
    }

    private void NewFrameReceived(int cameraIndex, Bitmap bitmap)
    {
        if (cameraIndex != (int)_trainCamInfo.CurrentCamera)
            return;

        CurrentView = bitmap;
    }
}