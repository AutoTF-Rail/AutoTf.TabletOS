using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TaskCompletionSource = System.Threading.Tasks.TaskCompletionSource;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class EasyControlView : UserControl
{
	private TaskCompletionSource _taskCompletionSource;
	private Grid _parent;
	
	public EasyControlView()
	{
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
}