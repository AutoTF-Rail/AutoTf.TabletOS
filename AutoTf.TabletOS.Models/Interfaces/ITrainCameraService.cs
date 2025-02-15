using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainCameraService : IDisposable
{
	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	public void DisconnectStreams();
	public Task StartListeningForCameras();
}