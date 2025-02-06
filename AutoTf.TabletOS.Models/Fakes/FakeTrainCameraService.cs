using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Models.Fakes;

public class FakeTrainCameraService : ITrainCameraService
{
	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	public void DisconnectStreams()
	{
		throw new NotImplementedException();
	}

	public Task StartListeningForCameras()
	{
		throw new NotImplementedException();
	}
}