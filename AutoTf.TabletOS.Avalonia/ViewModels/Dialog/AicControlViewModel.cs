using System;
using System.Threading.Tasks;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

public class AicControlViewModel : DialogViewModelBase
{
    private readonly IViewRouter _viewRouter;
    private readonly AicInformation _aicInformation;
    private readonly IAicService _aicService;
    private readonly Logger _logger;
    private readonly INotificationService _notificationService;

    private string _aicStatus = "Unavailable";
    private IImmutableSolidColorBrush _aicStatusBrush = Brushes.Black;
    private string _aicVersion = "Unavailable";
    private string _currentLocation = "0";
    private bool _interactionsEnabled;
    
    public string AicStatus
    {
        get => _aicStatus;
        private set => this.RaiseAndSetIfChanged(ref _aicStatus, value);
    }
    
    public IImmutableSolidColorBrush AicStatusBrush
    {
        get => _aicStatusBrush;
        private set => this.RaiseAndSetIfChanged(ref _aicStatusBrush, value);
    }
    
    public string AicVersion
    {
        get => _aicVersion;
        private set => this.RaiseAndSetIfChanged(ref _aicStatus, value);
    }
    
    public string CurrentLocation
    {
        get => _currentLocation;
        private set => this.RaiseAndSetIfChanged(ref _currentLocation, value);
    }
    
    public bool InteractionsEnabled
    {
        get => _interactionsEnabled;
        private set => this.RaiseAndSetIfChanged(ref _interactionsEnabled, value);
    }

    public IAsyncRelayCommand ShutdownCommand { get; }
    public IAsyncRelayCommand RestartCommand { get; }
    public IAsyncRelayCommand LogsCommand { get; }
    public IRelayCommand BackCommand { get; }
    
    public AicControlViewModel(IViewRouter viewRouter, AicInformation aicInformation, IAicService aicService, Logger logger, INotificationService notificationService)
    {
        _viewRouter = viewRouter;
        _aicInformation = aicInformation;
        _aicService = aicService;
        _logger = logger;
        _notificationService = notificationService;

        ShutdownCommand = new AsyncRelayCommand(ShutdownAic);
        RestartCommand = new AsyncRelayCommand(RestartAic);
        LogsCommand = new AsyncRelayCommand(OpenLogs);
        BackCommand = new RelayCommand(Back);
        
        Initialize();
    }

    private void Back()
    {
        Close(0);
    }

    private async Task OpenLogs()
    {
        Result<string[]> logResult = await _aicService.LogDates();
        
        if (!logResult.IsSuccess)
        {
            _notificationService.Warn("Could not get logs from AIC.");
            Back();
            return;
        }
        
        string[] logDates = logResult.Value!;
        
        await _viewRouter.ShowDialog(RemoteLogsViewerViewModel.Create(logDates, async s => await _aicService.Logs(s)));
    }

    private async Task RestartAic()
    {
        _viewRouter.InvokeLoadingArea(true, "Restarting AIC...");
        
        await _aicService.Restart();
        Back();
        _viewRouter.InvokeLoadingArea(false);
    }

    private async Task ShutdownAic()
    {
        _viewRouter.InvokeLoadingArea(true, "Shutting down AIC...");
        
        // TODO: Update status afterwards. (Wait for x seconds or listen for aic unavailablity)
        await _aicService.Shutdown();
        Back();
        _viewRouter.InvokeLoadingArea(false);
    }

    private async void Initialize()
    {
        try
        {
            _viewRouter.InvokeLoadingArea(true, "Loading data...");

            await _aicInformation.UpdateState();
            AicStatus = _aicInformation.State;
            AicStatusBrush = _aicInformation.Color;

            // TODO: Sync location
            CurrentLocation = _currentLocation;

            if (await _aicInformation.IsOnline())
                return;

            _interactionsEnabled = true;

            Result<string> versionResult = await _aicService.Version();
            AicVersion = versionResult.IsSuccess ? versionResult.Value! : "Unavailable";
        }
        catch (Exception e)
        {
            _logger.Log("An error occured while loading the AIC data:");
            _logger.Log(e.ToString());
            _notificationService.Error("An error occured while loading some AIC data.");
            Close(1);
        }
        finally
        {
            _viewRouter.InvokeLoadingArea(false);
        }
        
    }
}