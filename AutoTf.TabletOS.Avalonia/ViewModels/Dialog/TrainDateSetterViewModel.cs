using System;
using System.Threading.Tasks;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

public class TrainDateSetterViewModel : DialogViewModelBase
{
    private readonly ITrainInformationService _trainInformationService;
    private readonly Logger _logger;
    private readonly INotificationService _notificationService;

    private DateTimeOffset _selectedDate = DateTimeOffset.Now;
    private TimeSpan _selectedTime = DateTime.Now.TimeOfDay;

    public DateTimeOffset SelectedDate
    {
        get => _selectedDate;
        set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
    }

    public TimeSpan SelectedTime
    {
        get => _selectedTime;
        set => this.RaiseAndSetIfChanged(ref _selectedTime, value);
    }

    public IRelayCommand BackCommand { get; }
    public IAsyncRelayCommand SetCommand { get; }


    public TrainDateSetterViewModel(ITrainInformationService trainInformationService, Logger logger, INotificationService notificationService)
    {
        _trainInformationService = trainInformationService;
        _logger = logger;
        _notificationService = notificationService;

        BackCommand = new RelayCommand(Back);
        SetCommand = new AsyncRelayCommand(SetDateTime);
    }

    private async Task SetDateTime()
    {
        DateTime newDate = new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, _selectedTime.Hours, _selectedTime.Minutes, DateTime.Now.Second);
        _logger.Log($"Trying to replace date {DateTime.Now:O} with date {newDate:O}");
		
        Result dateResult = await _trainInformationService.SetDate(newDate);
        if (!dateResult.IsSuccess)
        {
            _notificationService.Error("Could not set date on train.");
            _logger.Log("Could not set date on train:");
            _logger.Log(dateResult.Error);
            return;
        }

        _notificationService.Success("Successfully set date on train.");
        Close(0);
    }

    private void Back()
    {
        Close(0);
    }
}