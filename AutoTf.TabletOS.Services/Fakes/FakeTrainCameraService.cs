using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeTrainCameraService : ITrainCameraService
{
	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	public void DisconnectStreams()
	{
	}

	public Task StartListeningForCameras()
	{
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		// TODO release managed resources here
	}
}