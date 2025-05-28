using System;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Logger = AutoTf.Logging.Logger;

namespace AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;

public class TrainInformationControlViewModel : ViewModelBase
{
    private readonly TrainInformation _trainInfo;
    private readonly INotificationService _notificationService;
    private readonly IViewRouter _viewRouter;
    private readonly Logger _logger;
    private readonly ITrainCameraService _trainCameraService;
    private readonly INetworkService _networkService;

    private string _evuName = "Loading";
    private string _trainName = "Loading";
    private string _trainId = "Loading";
    private string _trainVersion = "Loading";
    
    public string EvuName
    {
        get => _evuName;
        private set => this.RaiseAndSetIfChanged(ref _evuName, value);
    }
    
    public string TrainName
    {
        get => _trainName;
        private set => this.RaiseAndSetIfChanged(ref _trainName, value);
    }
    
    public string TrainId
    {
        get => _trainId;
        private set => this.RaiseAndSetIfChanged(ref _trainId, value);
    }
    
    public string TrainVersion
    {
        get => _trainVersion;
        private set => this.RaiseAndSetIfChanged(ref _trainVersion, value);
    }
    
    public IRelayCommand TrainInfoCommand { get; }
    
    public TrainInformationControlViewModel(TrainInformation trainInfo, INotificationService notificationService, IViewRouter viewRouter, Logger logger, ITrainCameraService trainCameraService, INetworkService networkService)
    {
        _trainInfo = trainInfo;
        _notificationService = notificationService;
        _viewRouter = viewRouter;
        _logger = logger;
        _trainCameraService = trainCameraService;
        _networkService = networkService;

        TrainInfoCommand = new AsyncRelayCommand(OpenTrainInfo);
    }
	
    private async Task OpenTrainInfo()
    {
        int result = await _viewRouter.ShowDialog<TrainInfoView, TrainInfoViewModel>();
        if (result == 1)
            ChangeToSelectionScreen();
    }
    
    private void ChangeToSelectionScreen()
    {
        try
        {
            _viewRouter.InvokeLoadingArea(true, "Disconnecting...");
            _logger.Log("Changing to train selection by request.");
			
            _trainCameraService.DisconnectStreams();
			
            // TODO: Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
            _networkService.ShutdownConnection();
        }
        catch (Exception e)
        {
            _logger.Log("Could not change to selection screen:");
            _logger.Log(e.ToString());
            _notificationService.Error("Could not disconnect from train.");
        }
    }

    protected override async Task Initialize()
    {
        await _trainInfo.UpdateData();
		
        if (!_trainInfo.InitializedSuccessfully)
        {
            _notificationService.Warn("Could not get all information from the train.");
        }

        EvuName = _trainInfo.EvuName;
        TrainName = _trainInfo.TrainName;
        TrainId = _trainInfo.TrainId;
        TrainVersion = _trainInfo.TrainVersion;
    }
}