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
}