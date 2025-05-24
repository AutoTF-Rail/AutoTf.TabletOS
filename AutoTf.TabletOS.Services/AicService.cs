using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class AicService : IAicService
{
    private const string AicBasePath = "http://192.168.0.1/aic";

    // The timeout is longer here because the timespan on the centralbridge is also 5 seconds, so it immediately kills itself
    
    public async Task<bool?> IsAvailable() => await HttpHelper.SendGet<bool?>(AicBasePath + "/isAvailable", false, 7);
    public async Task<bool> IsOnline() => await HttpHelper.SendGet<bool>(AicBasePath + "/online", false, 7);

    public async Task<string?> Version() => await HttpHelper.SendGet<string?>(AicBasePath + "/version", false, 7);
    public async Task<string[]> LogDates() => await HttpHelper.SendGet<string[]>(AicBasePath + "/information/logDates", false) ?? [];

    public async Task<string[]> Logs(string date) => await HttpHelper.SendGet<string[]>(AicBasePath + $"/information/logs?date={date}", false) ?? [];
    
    public void Shutdown() => _ = HttpHelper.SendPost(AicBasePath + "/shutdown", new StringContent(""), false);

    public void Restart() => _ = HttpHelper.SendPost(AicBasePath + "/restart", new StringContent(""), false);
}