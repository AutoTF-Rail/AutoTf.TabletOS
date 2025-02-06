using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Shared;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainGalleryView : UserControl
{
	private TaskCompletionSource _taskCompletionSource;
	private Grid _parent;
	
	public TrainGalleryView()
	{
		InitializeComponent();
		Initialize();
	}

	private void Initialize()
	{
		MediaView.MediaPlayer.Play(new Media(new LibVLC(),
			new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
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