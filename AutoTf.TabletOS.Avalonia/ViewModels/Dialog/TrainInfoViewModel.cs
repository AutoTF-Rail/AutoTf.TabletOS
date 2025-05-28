using System;
using System.Threading.Tasks;
using System.Timers;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

// 0: Do nothing
// 1: Change to train selection screen
public class TrainInfoViewModel : DialogViewModelBase
{
    private readonly ITrainInformationService _trainInfo;
    private readonly INotificationService _notificationService;
    private readonly IViewRouter _viewRouter;

    private string _evuName = "Loading";
    private string _trainId = "Loading";
    private string _trainName = "Loading";
    private string _trainVersion = "Loading";
    private string _lastTrainsync = "Loading";
    private string _nextTrainSync = "Loading";
    private IImmutableSolidColorBrush _nextSyncBrush = Brushes.Black; 
    
    private Timer? _saveTimer = new Timer(600);
    
    public string EvuName
    {
        get => _evuName;
        private set => this.RaiseAndSetIfChanged(ref _evuName, value);
    }
    
    public string TrainId
    {
        get => _trainId;
        private set => this.RaiseAndSetIfChanged(ref _trainId, value);
    }
    
    public string TrainName
    {
        get => _trainName;
        private set => this.RaiseAndSetIfChanged(ref _trainName, value);
    }
    
    public string TrainVersion
    {
        get => _trainVersion;
        private set => this.RaiseAndSetIfChanged(ref _trainVersion, value);
    }
    
    public string LastTrainSync
    {
        get => _lastTrainsync;
        private set => this.RaiseAndSetIfChanged(ref _lastTrainsync, value);
    }
    
    public string NextTrainSync
    {
        get => _nextTrainSync;
        private set => this.RaiseAndSetIfChanged(ref _nextTrainSync, value);
    }

    public string NextConnectionDay
    {
        get
        {
            if (!DateTime.TryParse(_lastTrainsync, out DateTime lastSync))
                return "Loading";
            
            int dayDiff = (lastSync - DateTime.Now).Days * -1;
            if (dayDiff >= 30)
                return "Long due.";
            
            return (30 - dayDiff) + " Days";
        }
    }
    
    public IImmutableSolidColorBrush NextSyncBrush 
    {
        get => _nextSyncBrush;
        private set => this.RaiseAndSetIfChanged(ref _nextSyncBrush, value);
    }
    
    public IAsyncRelayCommand ShutdownCommand { get; }
    public IAsyncRelayCommand RestartCommand { get; }
    public IAsyncRelayCommand UpdateCommand { get; }
    public IAsyncRelayCommand ChainCommand { get; }
    public IAsyncRelayCommand SetDateCommand { get; }
    public IAsyncRelayCommand LogsCommand { get; }
    
    public RelayCommand BackCommand { get; }
    

    public TrainInfoViewModel(ITrainInformationService trainInfo, INotificationService notificationService, IViewRouter viewRouter)
    {
        _trainInfo = trainInfo;
        _notificationService = notificationService;
        _viewRouter = viewRouter;

        ShutdownCommand = new AsyncRelayCommand(ShutdownTrain);
        RestartCommand = new AsyncRelayCommand(RestartTrain);
        UpdateCommand = new AsyncRelayCommand(UpdateTrain);
        ChainCommand = new AsyncRelayCommand(ChainTrain);
        SetDateCommand = new AsyncRelayCommand(SetTrainDate);
        LogsCommand = new AsyncRelayCommand(ViewLogs);
        
        BackCommand = new RelayCommand(Back);
    }

    private void Back()
    {
        Close(0);
    }

    private async Task ViewLogs()
    {
        string[] logDates = (await _trainInfo.GetLogDates()).GetValue([]);
		
        await _viewRouter.ShowDialog(RemoteLogsViewerViewModel.Create(logDates, async s => await _trainInfo.GetLogs(s)));
    }

    private async Task SetTrainDate()
    {
        await _viewRouter.ShowDialog<TrainDateSetterView, TrainDateSetterViewModel>();
    }

    private async Task ChainTrain()
    {
        await _viewRouter.ShowDialog<TrainChainView, TrainChainViewModel>();
    }

    private async Task UpdateTrain()
    {
        // TODO: Wait for completion
        await _trainInfo.PostUpdate();
        _notificationService.Info("A update has been triggered on the train. Please view the train logs to know when it has finished.");
    }

    private async Task RestartTrain()
    {
        _viewRouter.InvokeLoadingArea(true, "Restarting train...");
        
        // TODO: Notify train system of shutdown (seperate from actual shutdown?)
        // Prevent shutdown when train is still moving without assistant
        await _trainInfo.PostRestart();
        Close(1);
    }

    private async Task ShutdownTrain()
    {
        _viewRouter.InvokeLoadingArea(true, "Shutting down train...");
        
        // TODO: Notify train system of shutdown (seperate from actual shutdown?)
        // Prevent shutdown when train is still moving without assistant
        await _trainInfo.PostShutdown();
        Close(1);
    }

    protected override async Task Initialize()
    {
        string evuName = (await _trainInfo.GetEvuName()).GetValue("Unavailable");
        string trainId = (await _trainInfo.GetTrainId()).GetValue("Unavailable");
        string trainName = (await _trainInfo.GetTrainName()).GetValue("Unavailable");
        DateTime lastTrainSync = (await _trainInfo.GetLastSync()).GetValue(DateTime.MinValue);
        string trainVersion = (await _trainInfo.GetVersion()).GetValue("Unavailable");
        await UpdateSaveTimer();
		
        if (evuName == "Unavailable" || trainId == "Unavailable" || trainName == "Unavailable" || lastTrainSync == DateTime.MinValue || trainVersion == "Unavailable")
        {
            _notificationService.Warn("Could not get all train information data.");
            return;
        }

        int dayDiff = (lastTrainSync - DateTime.Now).Days * -1;

        IImmutableSolidColorBrush brush = ConvertDayIntoBrush(dayDiff);
        
        NextSyncBrush = brush;
        
        LastTrainSync = lastTrainSync.ToString("dd.MM.yyyy HH:ss");
        
        EvuName = evuName;
        TrainId = trainId;
        TrainName = trainName;
        TrainVersion = trainVersion;
    }

    // TODO: Is this even accurate anymore since we changed to ffmpeg?
    private async Task UpdateSaveTimer()
    {
        _saveTimer?.Dispose();
        DateTime nextSave = (await _trainInfo.GetNextSave()).GetValue(DateTime.MinValue);
        
        if (nextSave == DateTime.MinValue)
        {
            _notificationService.Warn("Could not get next train save date.");
            NextTrainSync = "Unknown";
            return;
        }
        
        int nextSaveInMs = (nextSave.Add(TimeSpan.FromSeconds(2)) - DateTime.Now).Milliseconds;
        if (nextSaveInMs <= 0)
        {
            NextTrainSync = "Past Due";
            return;
        }
        _saveTimer = new Timer(nextSaveInMs);
        _saveTimer.Elapsed += (_, _) => _ = UpdateSaveTimer();
		
        _saveTimer.Start();

        string date = nextSave.ToString("HH:mm:ss");
        NextTrainSync = date;
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
}