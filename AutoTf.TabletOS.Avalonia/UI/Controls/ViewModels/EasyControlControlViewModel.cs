using System;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;

public class EasyControlControlViewModel : ViewModelBase
{
    private readonly IViewRouter _viewRouter;
    private readonly INotificationService _notificationService;
    private readonly TrainCameraInformation _trainCamInfo;
    private readonly ITrainControlService _trainControl;
    private readonly Logger _logger;

    private bool _controlsUnavailable;
    private bool _emergencyStopUnavailable;
    
    public bool ControlsUnavailable
    {
        get => _controlsUnavailable;
        private set => this.RaiseAndSetIfChanged(ref _controlsUnavailable, value);
    }
    
    public bool EmergencyStopUnavailable
    {
        get => _emergencyStopUnavailable;
        private set => this.RaiseAndSetIfChanged(ref _emergencyStopUnavailable, value);
    }
    
    public IRelayCommand SpeedLimitCommand { get; }
    public IAsyncRelayCommand EmergencyStopCommand { get; }
    public IAsyncRelayCommand<string> EasyControlCommand { get; }
    public IAsyncRelayCommand ChangeDirectionCommand { get; }

    public EasyControlControlViewModel(IViewRouter viewRouter, INotificationService notificationService, TrainCameraInformation trainCamInfo, ITrainControlService trainControl, Logger logger)
    {
        _viewRouter = viewRouter;
        _notificationService = notificationService;
        _trainCamInfo = trainCamInfo;
        _trainControl = trainControl;
        _logger = logger;

        SpeedLimitCommand = new RelayCommand(ChangeSpeedLimit);
        EmergencyStopCommand = new AsyncRelayCommand(EmergencyStop);
        EasyControlCommand = new AsyncRelayCommand<string>(EasyControl);
        ChangeDirectionCommand = new AsyncRelayCommand(ChangeDirection);
    }

    protected override async Task Initialize()
    {
        // TODO: If available: Include check if its being driven by the AIC (If so: only disable EC, not emergency brake)
        bool ecAvailable = await _trainControl.IsEasyControlAvailable();
        ControlsUnavailable = !ecAvailable;
        EmergencyStopUnavailable = ecAvailable;
    }

    private async Task ChangeDirection()
    {
        try
        {
            await _viewRouter.InvokeLoadingArea(true, "Changing direction...");
		    
            // TODO: Can't change direction if train is actively moving. In the future just disable the button.
            if (false /*trainIsMoving*/)
            {
                _notificationService.Warn("Cannot change direction while train is moving.");
                return;
            }
		    
    #if DEBUG
            await Task.Delay(750);
    #endif
            
            // TODO: Notify train of side change and wait for completion
            if (_trainCamInfo.CurrentDirection == Side.Front)
                _trainCamInfo.CurrentDirection = Side.Back;
            else
                _trainCamInfo.CurrentDirection = Side.Front;

            switch (_trainCamInfo.CurrentDirection)
            {
                // If new direction is back, and current cam is front: Change cam
                case Side.Back when _trainCamInfo.CurrentCamera == Side.Front:
                // If new direction is front, and current cam is back: Change cam
                case Side.Front when _trainCamInfo.CurrentCamera == Side.Back:
                    ChangeCamera();
                    break;
            }

            await _viewRouter.InvokeLoadingArea(false);
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when changing direction:");
            _logger.Log(e.ToString());
            _notificationService.Error("Something went wrong when changing direction.");
        }
    }

    private void ChangeCamera()
    {
        _trainCamInfo.CurrentCamera = _trainCamInfo.CurrentCamera == Side.Front ? Side.Back : Side.Front;
    }

    private async Task EasyControl(string? value)
    {
        if (!int.TryParse(value, out int intValue))
            return;
        
        if (await _trainControl.EasyControl(intValue))
            return;
        
        _notificationService.Error($"Something went wrong when setting the EasyControl value {value}.");
    }

    private async Task EmergencyStop()
    {
        _logger.Log("Emergency brake has been invoked.");
        await _trainControl.EmergencyBrake();
    }

    private void ChangeSpeedLimit()
    {
        // TODO: Artificial speed limiter that is done by software too?
    }
}