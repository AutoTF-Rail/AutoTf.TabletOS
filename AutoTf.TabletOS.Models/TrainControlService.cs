using System.Text;
using System.Text.Json;
using AutoTf.Logging;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public class TrainControlService : ITrainControlService
{
	private readonly Logger _logger = Statics.Logger;
	
	public async Task<int> GetLeverCount()
	{
		try
		{
			string url = "http://192.168.1.1/control/levercount";

			using HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			return int.Parse(await response.Content.ReadAsStringAsync());
		}
		catch (Exception ex)
		{
			// TODO: Log
			return -1;
		}
	}

	public async Task<double> GetLeverPosition(int leverIndex)
	{
		try
		{
			string url = "http://192.168.1.1/control/leverPosition";

			using HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			return double.Parse(await response.Content.ReadAsStringAsync());
		}
		catch (Exception ex)
		{
			// TODO: Log
			return -1;
		}
	}

	public async Task<LeverType> GetLeverType(int leverIndex)
	{
		try
		{
			string url = "http://192.168.1.1/control/leverType";

			using HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());
			
			HttpResponseMessage response = await client.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			return Enum.Parse<LeverType>(await response.Content.ReadAsStringAsync());
		}
		catch (Exception ex)
		{
			// TODO: Log
			return LeverType.Unknown;
		}
	}

	public async Task<bool> SetLever(int leverIndex, double leverPercentage)
	{
		try
		{
			string url = "http://192.168.1.1/system/setLever";

			using HttpClient client = new HttpClient();
			
			client.DefaultRequestHeaders.Add("macAddr", Statics.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd());

			LeverSetModel leverModel = new LeverSetModel()
			{
				LeverIndex = leverIndex,
				Percentage = leverPercentage
			};

			HttpResponseMessage response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(leverModel), Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode)
			{
				_logger.Log("Could not set lever:");
				_logger.Log(await response.Content.ReadAsStringAsync());

				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.Log("Error while sending lever set request:");
			_logger.Log(ex.Message);
			return false;
		}
	}
}