using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

public class RemoteLogsViewerViewModel : DialogViewModelBase
{
    private ObservableCollection<string> _logs = new ObservableCollection<string>();
    private ObservableCollection<string> _logDates;
    private string _selectedDate = "";
    
    private readonly Func<string, Task<Result<string[]>>> _getLogs;
    
    public ObservableCollection<string> Logs
    {
        get => _logs;
        set => this.RaiseAndSetIfChanged(ref _logs, value);
    }
    
    public ObservableCollection<string> LogDates
    {
        get => _logDates;
        set => this.RaiseAndSetIfChanged(ref _logDates, value);
    }
    
    public string SelectedDate
    {
        get => _selectedDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDate, value);
            _ = LoadLogs(SelectedDate);
        }
    }

    public IRelayCommand BackCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public RemoteLogsViewerViewModel(string[] logsDates, Func<string, Task<Result<string[]>>> getLogs)
    {
        _logDates = new ObservableCollection<string>(logsDates);
        _getLogs = getLogs;
        
        BackCommand = new RelayCommand(Back);
        RefreshCommand = new AsyncRelayCommand(async () => await LoadLogs(SelectedDate));
        
        SelectedDate = LogDates.LastOrDefault() ?? "";
    }

    public static RemoteLogsViewer Create(string[] logsDates, Func<string, Task<Result<string[]>>> getLogs)
    {
        RemoteLogsViewerViewModel viewModel = new RemoteLogsViewerViewModel(logsDates, getLogs);
        return new RemoteLogsViewer(viewModel);
    }
    
    private async Task LoadLogs(string date)
    {
        Logs = new ObservableCollection<string>();
        string[] finalList = [];
		
        Result<string[]> logResult = await _getLogs.Invoke(date);
        if (logResult.IsSuccess)
            finalList = logResult.Value!;
		
        Logs = new ObservableCollection<string>(finalList);
    }

    private void Back()
    {
        Close(0);
    }
}