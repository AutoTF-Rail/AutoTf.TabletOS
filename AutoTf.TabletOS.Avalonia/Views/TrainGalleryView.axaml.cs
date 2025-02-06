using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainGalleryView : UserControl
{
	private TaskCompletionSource _taskCompletionSource;
	private Grid _parent;
	
	private WriteableBitmap _videoBitmap;
	private VideoCapture _videoCapture;
	private DispatcherTimer _frameTimer;
	private int _frameIndex;
	
	public TrainGalleryView()
	{
		InitializeComponent();
		Initialize();
	}

	private void Initialize()
	{
		_videoBitmap = new WriteableBitmap(new PixelSize(640, 480), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
		CameraViewBig.Source = _videoBitmap;

		_videoCapture = new VideoCapture("/Users/chiouya/Downloads/output-2025-02-05_20/35/35.mp4");
        
		_frameIndex = 0;
		_frameTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(1000.0 / 30) 
		};
		_frameTimer.Tick += OnFrameUpdate;
		_frameTimer.Start();
	}
	
	private async void OnFrameUpdate(object? sender, EventArgs e)
	{
		if (!_videoCapture.IsOpened)
			return;

		Mat frame = _videoCapture.QueryFrame();

		if (frame != null && !frame.IsEmpty)
		{
			using (var bitmap = frame.ToImage<Bgra, byte>()) 
			{
				UpdateFrame(bitmap);
			}
		}
		else
		{
			_frameTimer.Stop();
		}
	}
	
	private void UpdateFrame(Image<Bgra, byte> frame)
	{
		byte[] frameData = frame.Bytes;

		using (ILockedFramebuffer buf = _videoBitmap.Lock())
		{
			if (frameData.Length == buf.Size.Height * buf.Size.Width * 4)
			{
				unsafe
				{
					byte* ptr = (byte*)buf.Address;
					for (int i = 0; i < frameData.Length; i++)
					{
						ptr[i] = frameData[i];
					}
				}
			}
		}
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