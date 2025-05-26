using System;
using System.Threading.Tasks;
using System.Timers;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainInfoView : UserControl
{
	private TaskCompletionSource<int> _taskCompletionSource = null!;
	private Grid _parent = null!;
	
	private readonly ITrainInformationService _trainInfo;
	private Timer? _saveTimer = new Timer(600);
	
	public TrainInfoView(ITrainInformationService trainInfo)
	{
		_trainInfo = trainInfo;
		InitializeComponent();
		LoadInfoData();
	}
	// TODO: Is this even accurate anymore since we changed to ffmpeg?
	private async Task UpdateSaveTimer()
	{
		_saveTimer?.Dispose();
		DateTime nextSave = (await _trainInfo.GetNextSave()).GetValue(DateTime.MinValue);
		if (nextSave == DateTime.MinValue)
		{
			Statics.Notifications.Add(new Notification("Could not get next train save date.", Colors.Yellow));
			await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = "Unknown");
			return;
		}
		int nextSaveInMs = (nextSave.Add(TimeSpan.FromSeconds(2)) - DateTime.Now).Milliseconds;
		if (nextSaveInMs <= 0)
		{
			await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = "Past Due");
			return;
		}
		_saveTimer = new Timer(nextSaveInMs);
		_saveTimer.Elapsed += (_, _) => _ = UpdateSaveTimer();
		
		_saveTimer.Start();
		

		string date = nextSave.ToString("HH:mm:ss");
		await Dispatcher.UIThread.InvokeAsync(() => NextTrainSave.Text = date);
	}

	private async void LoadInfoData()
	{
		string evuName = (await _trainInfo.GetEvuName()).GetValue("Unavailable");
		string trainId = (await _trainInfo.GetTrainId()).GetValue("Unavailable");
		string trainName = (await _trainInfo.GetTrainName()).GetValue("Unavailable");
		DateTime lastTrainSync = (await _trainInfo.GetLastSync()).GetValue(DateTime.MinValue);
		string trainVersion = (await _trainInfo.GetVersion()).GetValue("Unavailable");
		await UpdateSaveTimer();
		
		if (evuName == "Unavailable" || trainId == "Unavailable" || trainName == "Unavailable" || lastTrainSync == DateTime.MinValue || trainVersion == "Unavailable")
		{
			Statics.Notifications.Add(new Notification("Could not get train information data. Please view the logs for more information.", Colors.Red));
			return;
		}

		int dayDiff = (lastTrainSync - DateTime.Now).Days * -1;

		IImmutableSolidColorBrush brush = ConvertDayIntoBrush(dayDiff);
		await Dispatcher.UIThread.InvokeAsync(() => NextTrainConnectionDay.Foreground = brush);

		if (dayDiff >= 30)
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextTrainConnectionDay.Text = $"Long due.");
		else
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextTrainConnectionDay.Text = (30 - dayDiff) + " Days");
		
		await Dispatcher.UIThread.InvokeAsync(() => EvuNameBox.Text = evuName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainIdBox.Text = trainId);
		await Dispatcher.UIThread.InvokeAsync(() => TrainNameBox.Text = trainName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainVersion.Text = trainVersion);
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

	public Task<int> Show(Grid parent)
	{
		_parent = parent;
		_taskCompletionSource = new TaskCompletionSource<int>();
		parent.Children.Add(this);

		return _taskCompletionSource.Task;
	}
	
	private void BackButton_Click(object? sender, RoutedEventArgs e)
	{
		SetResult(0);
	}

	// 0: Do nothing
	// 1: Change to train selection screen
	private void SetResult(int status)
	{
		_saveTimer?.Dispose();
		_parent.Children.Remove(this);
		_taskCompletionSource.TrySetResult(status);
	}
	
	private async void LogsButton_Click(object? sender, RoutedEventArgs e)
	{
		string[] logDates = (await _trainInfo.GetLogDates()).GetValue([]);
		
		RemoteLogsViewer logsView = new RemoteLogsViewer(logDates, async s => await _trainInfo.GetLogs(s));
		await logsView.Show(RootGrid);
	}
	
	private void SetDateButton_Click(object? sender, RoutedEventArgs e)
	{
		TrainDateSetter dateSetter = new TrainDateSetter(_trainInfo);
		dateSetter.Show(RootGrid);
	}

	private void ChainButton_Click(object? sender, RoutedEventArgs e)
	{
		TrainChainView chainView = new TrainChainView();
		chainView.Show(RootGrid);
	}
	
	private async void RestartTrain_Click(object? sender, RoutedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = "Restarting down train...";
			LoadingArea.IsVisible = true;
		});
		await Task.Delay(25);
		// TODO: Notify train system of shutdown (seperate from actual shutdown?)
		// Prevent shutdown when train is still moving without assistant
		await _trainInfo.PostRestart();
		SetResult(1);
	}

	private async void UpdateTrain_Click(object? sender, RoutedEventArgs e)
	{
		await _trainInfo.PostUpdate();
		Statics.Notifications.Add(new Notification("A update has been triggered on the train. Please view the train logs to know when it has finished.", Colors.Yellow));
	}

	private async void ShutdownTrain_Click(object? sender, RoutedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = "Shutting down train...";
			LoadingArea.IsVisible = true;
		});
		await Task.Delay(25);
		// TODO: Notify train system of shutdown (seperate from actual shutdown?)
		await _trainInfo.PostShutdown();
		// Prevent shutdown when train is still moving without assistant
		SetResult(1);
	}
}