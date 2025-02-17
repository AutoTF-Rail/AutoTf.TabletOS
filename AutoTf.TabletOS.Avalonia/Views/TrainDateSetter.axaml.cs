using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainDateSetter : UserControl
{
	private readonly Logger _logger = Statics.Logger;
	
	private readonly ITrainInformationService _trainInformationService;
	private TaskCompletionSource _taskCompletionSource = null!;
	private Grid _parent = null!;
	
	public TrainDateSetter(ITrainInformationService trainInformationService)
	{
		_trainInformationService = trainInformationService;
		InitializeComponent();
		DatePicker.SelectedDate = DateTimeOffset.Now;
		TimePicker.SelectedTime = DateTime.Now.TimeOfDay;
	}
	
	public Task Show(Grid parent)
	{
		_parent = parent;
		_taskCompletionSource = new TaskCompletionSource();
		parent.Children.Add(this);

		return _taskCompletionSource.Task;
	}
	
	private void BackButton_Click(object? sender, RoutedEventArgs e)
	{
		_parent.Children.Remove(this);
		_taskCompletionSource.TrySetResult();
	}

	private async void SetButton_Click(object? sender, RoutedEventArgs e)
	{
		if (DatePicker.SelectedDate == null)
			return;
		if (TimePicker.SelectedTime == null)
			return;
		
		DateTime selectedDate = DatePicker.SelectedDate.Value.DateTime;
		TimeSpan selectedTime = TimePicker.SelectedTime.Value;

		DateTime newDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, selectedTime.Hours, selectedTime.Minutes, DateTime.Now.Second);
		_logger.Log($"Trying to replace date {DateTime.Now:O} with date {newDate:O}");
		
		bool dateResult = await _trainInformationService.SetDate(newDate);
		if (!dateResult)
		{
			Statics.Notifications.Add(new Notification("Could not set date on train.", Colors.Red));
			_logger.Log("Could not set date on train.");
			return;
		}

		_logger.Log("Successfully set date on train.");
		Statics.Notifications.Add(new Notification(CommandExecuter.ExecuteCommand($"date -s \"{newDate:yyyy-MM-dd HH:mm:ss}\""), Colors.Yellow));
		
		Statics.Shutdown?.Invoke();
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "shutdown",
				Arguments = "now",
				CreateNoWindow = true,
				UseShellExecute = false
			}
		};
		
		process.Start();
		Environment.Exit(0);
	}
}