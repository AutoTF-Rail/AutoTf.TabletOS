using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class AicService : IAicService
{
    private const string AicBasePath = "http://192.168.0.1/aic";

    // The timeout for those requests on the CB is set to 3 seconds, so in total it is 6 seconds it could take MAX
    public async Task<Result<bool?>> IsAvailable() => await HttpHelper.SendGet<bool?>(AicBasePath + "/isAvailable", 6);
    public async Task<Result> IsOnline() => await HttpHelper.SendGetRaw(AicBasePath + "/", 6);
    public async Task<Result<string>> Version() => await HttpHelper.SendGet(AicBasePath + "/version", 6);
    public async Task<Result<string[]>> LogDates() => await HttpHelper.SendGet<string[]>(AicBasePath + "/information/logDates", 7);
    public async Task<Result<string[]>> Logs(string date) => await HttpHelper.SendGet<string[]>(AicBasePath + $"/information/logs?date={date}", 7);
    
    public async Task<Result> Shutdown() => await HttpHelper.SendPost(AicBasePath + "/shutdown", new StringContent(""));

    public async Task<Result> Restart() => await HttpHelper.SendPost(AicBasePath + "/restart", new StringContent(""));
}