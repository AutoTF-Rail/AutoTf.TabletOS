using System.Net.Sockets;
using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Models;

public class TrainCameraService : ITrainCameraService
{
	private readonly Logger _logger = Statics.Logger;
	
	private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;

	private List<UdpClient> _udpClients = new List<UdpClient>();

	private List<Bitmap?> _currentBitmaps = new List<Bitmap?>();

	// TODO: Make global cancellation token too for shutdown?
	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	
	public async void DisconnectStreams()
	{
		_logger.Log("TCS: Stream disconnect was requested.");
		await _cancellationTokenSource.CancelAsync();
	}

	public TrainCameraService()
	{
		Statics.Shutdown += DisconnectStreams;
	}

	// TODO: "OnTrainConnected" event for things like this:
	public async Task StartListeningForCameras()
	{
		try
		{
			int? cameraCount = await _trainInfo.GetCameraCount();
			if (cameraCount == null)
			{
				Statics.Notifications.Add(new Notification("Could not find any cameras.", Colors.Red));
				_logger.Log("TCS: Camera count was null.");
				return;
			}
			
			_logger.Log($"TCS: Found {cameraCount} cameras.");

			for (int i = 0; i < cameraCount; i++)
			{
				_logger.Log($"TCS: Starting stream for camera {i}.");

                await Task.Run(() => ListenForStream(i));
			}
		}
		catch (Exception ex)
		{
			Statics.Notifications.Add(new Notification("An error occured while starting up the camera connection.", Colors.Red));
			_logger.Log("TCS: Error while getting UDP stream:");
			_logger.Log(ex.ToString());
		}
	}

	private async void ListenForStream(int cameraIndex)
	{
		int udpPort = 1234 + cameraIndex;
		
		if (!await PostStartStream(udpPort, cameraIndex))
			return;
		
		_logger.Log($"TCS: Listening for images for camera {cameraIndex} on port {udpPort}");
		_currentBitmaps.Add(null);
		
		UdpClient udpClient = new UdpClient(udpPort);
        _udpClients.Add(udpClient);

        while (_cancellationTokenSource.IsCancellationRequested)
        {
	        try
	        {
	            UdpReceiveResult result = await udpClient.ReceiveAsync();
	            byte[] frameData = result.Buffer;

	            if (frameData.Length == 0)
	            {
	                _logger.Log($"TCS: Received empty frame data from camera {cameraIndex}.");
	                Thread.Sleep(25);
	                continue;
	            }
	            
	            using (MemoryStream ms = new MemoryStream(frameData))
	            {
	                if (ms.Length > 0)
	                {
	                    try
	                    {
	                        Bitmap? oldBitmap = _currentBitmaps[cameraIndex];
	                        
	                        _currentBitmaps[cameraIndex] = new Bitmap(ms);
	                        
							NewFrameReceived?.Invoke(cameraIndex, _currentBitmaps[cameraIndex]!);

	                        if (oldBitmap != null && oldBitmap != _currentBitmaps[cameraIndex])
	                        {
                        		oldBitmap.Dispose();
	                        }
	                    }
	                    catch (Exception ex)
	                    {
	                        _logger.Log($"TCS: Error creating bitmap: {ex.Message}");
	                    }
	                }
	            }
	        }
	        catch (Exception e)
	        {
		        _logger.Log($"TCS: Error during stream receival:");
		        _logger.Log(e.ToString());
	        }
        }
		
        _logger.Log("Stopping stream.");
        _udpClients[cameraIndex].Dispose();
	    await PostStopStream(cameraIndex);
	}
	
	private async Task<bool> PostStartStream(int port, int cameraIndex)
	{
		try
		{
			_logger.Log($"TCS: Requesting stream start with port {port} and camera index {cameraIndex}.");
			string serverUrl = $"http://192.168.1.1/camera/startStream?port={port}&cameraIndex={cameraIndex}";

			using (HttpClient client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromSeconds(5);
				HttpResponseMessage response = await client.PostAsync(serverUrl, null);

				if (!response.IsSuccessStatusCode)
				{
					Statics.Notifications.Add(new Notification($"Could not start the stream for camera {cameraIndex}.", Colors.Red));
					_logger.Log($"TCS: {response.StatusCode}: Could not start stream:");
					_logger.Log(await response.Content.ReadAsStringAsync());
				}

				return response.IsSuccessStatusCode;
			}
		}
		catch (Exception e)
		{
			Statics.Notifications.Add(new Notification($"Could not start the stream for camera {cameraIndex}.", Colors.Red));
			_logger.Log("TCS: Failed to start stream:");
			_logger.Log(e.ToString());
		}

		return false;
	}

	private async Task<bool> PostStopStream(int cameraIndex)
	{
		try
		{
			string serverUrl = $"http://192.168.1.1/camera/stopStream?cameraIndex={cameraIndex}";
			
			using (HttpClient client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromSeconds(5);
				HttpResponseMessage response = await client.PostAsync(serverUrl, null);

				if(!response.IsSuccessStatusCode)
					_logger.Log("TCS: Could not stop stream: " + await response.Content.ReadAsStringAsync());
			
				return response.IsSuccessStatusCode;
			}
		}
		catch (Exception e)
		{
			Statics.Notifications.Add(new Notification($"Could not stop the stream for camera {cameraIndex}.", Colors.Red));
			_logger.Log("TCS: Error while stopping stream:");
			_logger.Log(e.ToString());
		}

		return false;
	}
}