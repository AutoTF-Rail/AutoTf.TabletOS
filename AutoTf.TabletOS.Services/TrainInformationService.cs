using System.Net.Http.Json;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class TrainInformationService : ITrainInformationService
{
	private readonly Logger _logger = Statics.Logger;

	public async Task<string?> GetEvuName()
	{
		try
		{
			string url = "http://192.168.1.1/information/evuname";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get EVU Name:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<string?> GetTrainId()
	{
		try
		{
			string url = "http://192.168.1.1/information/trainId";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get train Id:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<string?> GetTrainName()
	{
		try
		{
			string url = "http://192.168.1.1/information/trainName";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get train name:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<string?> GetLastSync()
	{
		try
		{
			string url = "http://192.168.1.1/information/lastsynced";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get last sync try:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<string?> GetVersion()
	{
		try
		{
			string url = "http://192.168.1.1/information/version";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			
			return content;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get version:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<string[]?> GetLogDates()
	{
		try
		{
			string url = "http://192.168.1.1/information/logdates";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string[]? dates = await response.Content.ReadFromJsonAsync<string[]>();

			return dates;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get log dates:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<string[]?> GetLogs(string date)
	{
		try
		{
			string url = "http://192.168.1.1/information/logs?date=" + date;

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string[]? logs = await response.Content.ReadFromJsonAsync<string[]>();

			return logs;
		}
		catch (Exception ex)
		{
			_logger.Log($"TIS: Could not get logs for {date}:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<DateTime?> GetNextSave()
	{
		try
		{
			string url = "http://192.168.1.1/camera/nextSave";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			return DateTime.Parse(await response.Content.ReadAsStringAsync());
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not get next save date:");
			_logger.Log(ex.ToString());
			return null;
		}
	}

	public async Task<bool> PostUpdate()
	{
		try
		{
			string url = "http://192.168.1.1/system/update";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("Could not send update signal:");
				_logger.Log(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS:Could not send update signal:");
			_logger.Log(ex.ToString());
			return false;
		}
	}

	public async Task<bool> PostShutdown()
	{
		try
		{
			string url = "http://192.168.1.1/system/shutdown";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("TIS: Could not send shutdown signal:");
				_logger.Log(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS:Could not send shutdown signal:");
			_logger.Log(ex.ToString());
			return false;
		}
	}

	public async Task<bool> PostRestart()
	{
		try
		{
			string url = "http://192.168.1.1/system/restart";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(""));
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("TIS: Could not send restart signal:");
				_logger.Log(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not send restart signal:");
			_logger.Log(ex.ToString());
			return false;
		}
	}

	public async Task<bool> SetDate(DateTime date)
	{
		try
		{
			string url = "http://192.168.1.1/system/setdate";

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(5);
			
			client.DefaultRequestHeaders.Add("macAddr", Statics.MacAddress);
			
			HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(date));;
			
			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("TIS: Could not set date on train:");
				_logger.Log(response.StatusCode + ":" + await response.Content.ReadAsStringAsync());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("TIS: Could not set date on train:");
			_logger.Log(ex.ToString());
			return false;
		}
	}
}