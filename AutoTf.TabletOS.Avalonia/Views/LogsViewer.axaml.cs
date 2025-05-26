using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class LogsViewer : UserControl
{
	private TaskCompletionSource _taskCompletionSource = null!;
	private Grid _parent = null!;
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
		string[] files = [];
		
		if (Path.Exists(_logDir))
			files = Directory.GetFiles(_logDir).Order().ToArray();
		
		DateBox.ItemsSource = files.Select(Path.GetFileNameWithoutExtension);
		DateBox.SelectedIndex = DateBox.ItemCount - 1;
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
		LogViewerBox.ItemsSource = File.ReadAllLines(_logDir + (string)DateBox.SelectedItem! + ".txt");
		ScrollViewer? scrollViewer = LogViewerBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
		scrollViewer?.ScrollToEnd();
	}

	private void LogViewerBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		LogViewerBox.SelectedItem = null;
	}
}