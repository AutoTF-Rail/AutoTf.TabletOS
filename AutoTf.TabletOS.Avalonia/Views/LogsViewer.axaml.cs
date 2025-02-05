using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class LogsViewer : UserControl
{
	private TaskCompletionSource _taskCompletionSource;
	private Grid _parent;
	private readonly string _logDir = "/var/log/AutoTF/AutoTf.TabletOS.Avalonia/";
	
	public LogsViewer()
	{
		InitializeComponent();
		Initialize();
	}

	public Task Show(Grid parent)
	{
		_parent = parent;
		_taskCompletionSource = new TaskCompletionSource();
		parent.Children.Add(this);

		return _taskCompletionSource.Task;
	}

	private void Initialize()
	{
		// DateBox.Items.Add(DateTime.Now.ToString("yyyy-MM-dd"));
		
		string[] files = Directory.GetFiles(_logDir).Order().ToArray();
		DateBox.ItemsSource = files.Select(Path.GetFileNameWithoutExtension);
		
		
		
		DateBox.SelectedIndex = 0;
	}

	private void Close()
	{
		_parent.Children.Remove(this);
	}

	private void BackButton_Click(object? sender, RoutedEventArgs e)
	{
		Close();
		_taskCompletionSource.TrySetResult();
	}

	private void DateBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		LogViewerBox.ItemsSource = File.ReadAllLines(_logDir + (string)DateBox.SelectedItem! + ".txt").Reverse();
	}
}