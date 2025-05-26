using System;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.CentralBridge.Shared.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class RemoteLogsViewer : UserControl
{
	private readonly string[] _logsDates;
	private readonly Func<string, Task<Result<string[]>>> _getLogs;
	
	private TaskCompletionSource _taskCompletionSource = null!;
	private Grid _parent = null!;
	
	public RemoteLogsViewer(string[] logsDates, Func<string, Task<Result<string[]>>> getLogs)
	{
		_logsDates = logsDates;
		_getLogs = getLogs;
		InitializeComponent();
		Initialize();
	}

	private void Initialize()
	{
		DateBox.ItemsSource = _logsDates;
		
		DateBox.SelectedIndex = DateBox.ItemCount - 1;
	}

	private async void DateBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		await Dispatcher.UIThread.InvokeAsync(async () =>
		{
			string[] finalList = [];
			
			Result<string[]> logResult = await _getLogs.Invoke((string)DateBox.SelectedItem!);
			if (logResult.IsSuccess)
				finalList = logResult.Value!;
			
			LogViewerBox.ItemsSource = finalList;
		});
		
		ScrollViewer? scrollViewer = LogViewerBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
		scrollViewer?.ScrollToEnd();
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