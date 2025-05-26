using System.Net.Sockets;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AutoTf.TabletOS.Services;

public class TrainCameraService : ITrainCameraService
{
	private readonly Logger _logger = Statics.Logger;

	private readonly List<UdpClient> _udpClients = new List<UdpClient>();
	private readonly List<Bitmap?> _currentBitmaps = new List<Bitmap?>();
	
	private bool _canStream = true;

	public Action<int, Bitmap>? NewFrameReceived { get; set; }
	
	public async void DisconnectStreams()
	{
		_logger.Log("TCS: Stream disconnect was requested.");
		_canStream = false;
		for (int i = 0; i < _udpClients.Count; i++)
		{
			if(!(await PostStopStream(i)).IsSuccess)
				_logger.Log($"Could not start stream for camera at index {i}.");
		}
		
		foreach (var udpClient in _udpClients)
		{
			udpClient.Close();  // Close the socket
			udpClient.Dispose(); // Ensure it's fully released
		}
		_udpClients.Clear();
	}

	// TODO: "OnTrainConnected" event for things like this:
	public async Task StartListeningForCameras()
	{
		try
		{
			if (!await PostStartStream(1234))
				return;

			for (int i = 0; i < 2; i++)
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
		
		_logger.Log($"TCS: Listening for images for camera {cameraIndex} on port {udpPort}");
		_currentBitmaps.Add(null);
		
		UdpClient udpClient = new UdpClient(udpPort);
		_udpClients.Add(udpClient);

        while (_canStream)
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

	            using MemoryStream ms = new MemoryStream(frameData);

	            if (ms.Length <= 0) 
		            continue;
	            
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
	        catch (Exception e)
	        {
		        _logger.Log($"TCS: Error during stream receival:");
		        _logger.Log(e.ToString());
	        }
        }
		
        _logger.Log("Stopping stream: Can stream: " + _canStream);
	}
	
	private async Task<bool> PostStartStream(int port)
	{
		try
		{
			_logger.Log($"TCS: Requesting stream start with port {port}.");
			string serverUrl = $"http://192.168.1.1/camera/startStream?port={port}";

			using HttpClient client = new HttpClient();
			
			client.Timeout = TimeSpan.FromSeconds(5);
			HttpResponseMessage response = await client.PostAsync(serverUrl, null);

			if (response.IsSuccessStatusCode) 
				return true;
			
			// Statics.Notifications.Add(new Notification($"Could not start the stream.", Colors.Red));
			_logger.Log($"TCS: {response.StatusCode}: Could not start stream:");
			_logger.Log(await response.Content.ReadAsStringAsync());
			return false;

		}
		catch (Exception e)
		{
			// Statics.Notifications.Add(new Notification($"Could not start the stream.", Colors.Red));
			_logger.Log("TCS: Failed to start stream:");
			_logger.Log(e.ToString());
		}

		return false;
	}

	private async Task<Result> PostStopStream(int cameraIndex)
	{
		return await HttpHelper.SendPost($"http://192.168.1.1/camera/stopStream?cameraIndex={cameraIndex}", new StringContent(""));
	}
}