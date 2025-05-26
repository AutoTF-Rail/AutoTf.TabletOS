using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class AicService : IAicService
{
    private const string AicBasePath = "http://192.168.0.1/aic";

    // The timeout is longer here because the timespan on the centralbridge is also 5 seconds, so it immediately kills itself
    public async Task<Result<bool?>> IsAvailable() => await HttpHelper.SendGet<bool?>(AicBasePath + "/isAvailable", 7);
    public async Task<Result> IsOnline() => await HttpHelper.SendGetRaw(AicBasePath + "/", 7);
    public async Task<Result<string>> Version() => await HttpHelper.SendGet(AicBasePath + "/version", 7);
    public async Task<Result<string[]>> LogDates() => await HttpHelper.SendGet<string[]>(AicBasePath + "/information/logDates");
    public async Task<Result<string[]>> Logs(string date) => await HttpHelper.SendGet<string[]>(AicBasePath + $"/information/logs?date={date}");
    
    public async Task<Result> Shutdown() => await HttpHelper.SendPost(AicBasePath + "/shutdown", new StringContent(""));

    public async Task<Result> Restart() => await HttpHelper.SendPost(AicBasePath + "/restart", new StringContent(""));
}