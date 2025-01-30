using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public class TrainInformationService : ITrainInformationService
{
	private readonly Logger _logger = Statics.Logger;
	
	public async Task<string?> GetEvuName()
	{
		try
		{
			string url = "http://192.168.1.1/information/evuname";

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			// TODO: Log
			return null;
		}
	}

	public async Task<string?> GetTrainId()
	{
		try
		{
			string url = "http://192.168.1.1/information/trainId";

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			// TODO: Log
			return null;
		}
	}

	public async Task<string?> GetTrainName()
	{
		try
		{
			string url = "http://192.168.1.1/information/trainName";

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			// TODO: Log
			return null;
		}
	}

	public async Task<string?> GetLastSync()
	{
		try
		{
			string url = "http://192.168.1.1/information/lastsynced";

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			// TODO: Log
			return null;
		}
	}

	public async Task<string?> GetVersion()
	{
		try
		{
			string url = "http://192.168.1.1/information/version";

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			// TODO: Log
			return null;
		}
	}

	public async Task<DateTime?> GetNextSave()
	{
		try
		{
			string url = "http://192.168.1.1/camera/nextSave";

			using HttpClient loginClient = new HttpClient();
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			return DateTime.Parse(await response.Content.ReadAsStringAsync());
		}
		catch (Exception ex)
		{
			// TODO: Log
			return null;
		}
	}

	public async Task<bool> PostUpdate()
	{
		try
		{
			string url = "http://192.168.1.1/update";

			using HttpClient client = new HttpClient();
			// TODO: Cache mac address
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("Could not send update signal:");
				_logger.Log(await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("Could not send update signal:");
			_logger.Log(ex.Message);
			return false;
		}
	}

	public async Task<bool> PostShutdown()
	{
		try
		{
			string url = "http://192.168.1.1/shutdown";

			using HttpClient client = new HttpClient();
			// TODO: Cache mac address
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("Could not send shutdown signal:");
				_logger.Log(await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("Could not send shutdown signal:");
			_logger.Log(ex.Message);
			return false;
		}
	}

	public async Task<bool> PostRestart()
	{
		try
		{
			string url = "http://192.168.1.1/restart";

			using HttpClient client = new HttpClient();
			// TODO: Cache mac address
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("Could not send restart signal:");
				_logger.Log(await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("Could not send restart signal:");
			_logger.Log(ex.Message);
			return false;
		}
	}
}