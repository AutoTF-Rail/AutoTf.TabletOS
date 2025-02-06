using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainCameraService
{
	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	public void DisconnectStreams();
}