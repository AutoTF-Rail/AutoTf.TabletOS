using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public class TrainInformationService : ITrainInformationService
{
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

	public async Task PostUpdate()
	{
		try
		{
			string url = "http://192.168.1.1/update";

			using HttpClient client = new HttpClient();
			// TODO: Cache mac address
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
		}
		catch (Exception ex)
		{
			// TODO: Log
		}
	}

	public async Task PostShutdown()
	{
		try
		{
			string url = "http://192.168.1.1/shutdown";

			using HttpClient client = new HttpClient();
			// TODO: Cache mac address
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
		}
		catch (Exception ex)
		{
			// TODO: Log
		}
	}

	public async Task PostRestart()
	{
		try
		{
			string url = "http://192.168.1.1/restart";

			using HttpClient client = new HttpClient();
			// TODO: Cache mac address
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
		}
		catch (Exception ex)
		{
			// TODO: Log
		}
	}
}