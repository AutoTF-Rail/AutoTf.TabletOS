using System.Text;
using System.Text.Json;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class TrainInformationService : ITrainInformationService
{
	private readonly string _infoBaseUrl = "http://192.168.1.1/information";
	private readonly string _camBaseUrl = "http://192.168.1.1/camera";
	private readonly string _sysBaseUrl = "http://192.168.1.1/system";

	public async Task<string?> GetEvuName() => await HttpHelper.SendGet<string?>(_infoBaseUrl + "/evuname", false);

	public async Task<string?> GetTrainId() => await HttpHelper.SendGet<string?>(_infoBaseUrl + "/trainId", false);

	public async Task<string?> GetTrainName() => await HttpHelper.SendGet<string?>(_infoBaseUrl + "/trainName", false);

	public async Task<string?> GetLastSync() => await HttpHelper.SendGet<string?>(_infoBaseUrl + "/lastsynced", false);

	public async Task<string?> GetVersion() => await HttpHelper.SendGet<string?>(_infoBaseUrl + "/version", false);

	public async Task<string[]?> GetLogDates() => await HttpHelper.SendGet<string[]?>(_infoBaseUrl + "/logdates", false);

	public async Task<string[]?> GetLogs(string date) => await HttpHelper.SendGet<string[]?>(_infoBaseUrl + "/logs?date=" + date, false);

	public async Task<DateTime?> GetNextSave() => await HttpHelper.SendGet<DateTime?>(_camBaseUrl + "/nextSave", false);

	public async Task<bool> PostUpdate() => await HttpHelper.SendPost(_sysBaseUrl + "/update", new StringContent(""));

	public async Task<bool> PostShutdown() => await HttpHelper.SendPost(_sysBaseUrl + "/shutdown", new StringContent(""));

	public async Task<bool> PostRestart() => await HttpHelper.SendPost(_sysBaseUrl + "/restart", new StringContent(""));

	public async Task<bool> SetDate(DateTime date) => await HttpHelper.SendPost(_sysBaseUrl + "/setdate", new StringContent(JsonSerializer.Serialize(date), Encoding.UTF8, "application/json"));
}