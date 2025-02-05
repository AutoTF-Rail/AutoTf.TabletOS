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
		string dir = "/var/log/AutoTF/AutoTf.TabletOS.Avalonia/";
		string[] files = Directory.GetFiles(dir);
		DateBox.ItemsSource = files.Select(Path.GetFileNameWithoutExtension);
		
		// string path = dir + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
		//
		// if (!File.Exists(path))
		// {
		// 	LogViewerBox.Items.Add("No logs found");
		// }
		//
		// string[] logs = File.ReadAllLines(path);
		// LogViewerBox.ItemsSource = logs;
		//
		
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
}