using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainLogsViewer : UserControl
{
	private readonly ITrainInformationService _trainInformationService;
	private TaskCompletionSource _taskCompletionSource;
	private Grid _parent;
	
	public TrainLogsViewer(ITrainInformationService trainInformationService)
	{
		_trainInformationService = trainInformationService;
		InitializeComponent();
		Initialize();
	}

	private async void Initialize()
	{
		string[]? dates = await _trainInformationService.GetLogDates();
		if (dates == null)
			return;
		
		DateBox.ItemsSource = dates;
		
		DateBox.SelectedIndex = 0;
	}

	private async void DateBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		await Dispatcher.UIThread.Invoke(async () => LogViewerBox.ItemsSource = await _trainInformationService.GetLogs((string)DateBox.SelectedItem!));
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

	private void LogViewerBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		LogViewerBox.SelectedItem = null;
	}
}