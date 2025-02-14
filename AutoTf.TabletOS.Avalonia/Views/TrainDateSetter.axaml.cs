using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainDateSetter : UserControl
{
	private readonly ITrainInformationService _trainInformationService;
	private TaskCompletionSource _taskCompletionSource;
	private Grid _parent;
	
	public TrainDateSetter(ITrainInformationService trainInformationService)
	{
		_trainInformationService = trainInformationService;
		InitializeComponent();
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

		bool dateResult = await _trainInformationService.SetDate(DatePicker.SelectedDate.Value.DateTime);
		if (!dateResult)
		{
			Statics.Notifications.Add(new Notification("Could not set date on train.", Colors.Red));
			return;
		}

		Statics.Notifications.Add(new Notification(CommandExecuter.ExecuteCommand($"date -s \"{DatePicker.SelectedDate:yyyy-MM-dd HH:mm:ss}\""), Colors.Yellow));
		// The connection is dead anyways.
		// _networkManager.ShutdownConnection();
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