using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class Popup : UserControl
{
	private TaskCompletionSource<DialogResult> _taskCompletionSource;
	private Grid _parent;
	
	public Popup(string content, bool showCancel = false)
	{
		InitializeComponent();

		Question.Text = content;
		CancelButton.IsVisible = showCancel;

		StaticEvents.BrightnessChanged += BrightnessChanged;
	}

	private void BrightnessChanged()
	{
		this.Opacity = StaticEvents.CurrentBrightness;
	}
	
	public Task<DialogResult> Show(Grid parent)
	{
		_parent = parent;
		_taskCompletionSource = new TaskCompletionSource<DialogResult>();
		parent.Children.Add(this);

		return _taskCompletionSource.Task;
	}

	private void YesButton_OnClick(object? sender, RoutedEventArgs e)
	{
		Close();
		_taskCompletionSource.TrySetResult(DialogResult.Yes);
	}

	private void NoButton_OnClick(object? sender, RoutedEventArgs e)
	{
		Close();
		_taskCompletionSource.TrySetResult(DialogResult.No);
	}

	private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
	{
		Close();
		_taskCompletionSource.TrySetResult(DialogResult.Cancel);
	}

	private void Close()
	{
		_parent.Children.Remove(this);
	}
}