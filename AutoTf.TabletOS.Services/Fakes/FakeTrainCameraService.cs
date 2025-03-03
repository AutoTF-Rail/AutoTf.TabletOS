using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeTrainCameraService : ITrainCameraService
{
	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	private int _iteration = 0;
	
	public void DisconnectStreams()
	{
	}

	public Task StartListeningForCameras()
	{
		if (_iteration == 0)
		{
			NewFrameReceived?.Invoke(0, new Bitmap("CopiedAssets/ExampleImage1.png"));
			_iteration = 1;
		}
		else
		{
			NewFrameReceived?.Invoke(1, new Bitmap("CopiedAssets/ExampleImage2.png"));
			_iteration = 0;
		}

		return Task.CompletedTask;
	}

	public void Dispose()
	{
		// TODO release managed resources here
	}
}