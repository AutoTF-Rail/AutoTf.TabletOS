using System.Text;
using System.Text.Json;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class TrainInformationService : ITrainInformationService
{
	private readonly string _infoBaseUrl = "http://192.168.1.1/information";
	private readonly string _camBaseUrl = "http://192.168.1.1/camera";
	private readonly string _sysBaseUrl = "http://192.168.1.1/system";

	public async Task<Result<string>> GetEvuName() => await HttpHelper.SendGet(_infoBaseUrl + "/evuname");

	public async Task<Result<string>> GetTrainId() => await HttpHelper.SendGet(_infoBaseUrl + "/trainId");

	public async Task<Result<string>> GetTrainName() => await HttpHelper.SendGet(_infoBaseUrl + "/trainName");

	public async Task<Result<DateTime>> GetLastSync() => await HttpHelper.SendGet<DateTime>(_infoBaseUrl + "/lastsynced");

	public async Task<Result<string>> GetVersion() => await HttpHelper.SendGet(_infoBaseUrl + "/version");

	public async Task<Result<string[]>> GetLogDates() => await HttpHelper.SendGet<string[]>(_infoBaseUrl + "/logdates");

	public async Task<Result<string[]>> GetLogs(string date) => await HttpHelper.SendGet<string[]>(_infoBaseUrl + "/logs?date=" + date);

	public async Task<Result<DateTime>> GetNextSave() => await HttpHelper.SendGet<DateTime>(_camBaseUrl + "/nextSave");

	public async Task<Result> PostUpdate() => await HttpHelper.SendPost(_sysBaseUrl + "/update", new StringContent(""));

	public async Task<Result> PostShutdown() => await HttpHelper.SendPost(_sysBaseUrl + "/shutdown", new StringContent(""));

	public async Task<Result> PostRestart() => await HttpHelper.SendPost(_sysBaseUrl + "/restart", new StringContent(""));

	public async Task<Result> SetDate(DateTime date) => await HttpHelper.SendPost(_sysBaseUrl + "/setdate", new StringContent(JsonSerializer.Serialize(date), Encoding.UTF8, "application/json"));
}