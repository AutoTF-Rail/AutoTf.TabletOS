using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class AicService : IAicService
{
    private const string AicBasePath = "http://192.168.0.1/aic";
    
    public async Task<bool?> IsAvailable() => await HttpHelper.SendGet<bool?>(AicBasePath + "/isAvailable");

    public async Task<bool> IsOnline() => await HttpHelper.SendGet<bool>(AicBasePath + "/online");

    public async Task<string?> Version() => await HttpHelper.SendGet<string?>(AicBasePath + "/version");

    public void Shutdown() => _ = HttpHelper.SendPost(AicBasePath + "/shutdown", new StringContent(""), false);

    public void Restart() => _ = HttpHelper.SendPost(AicBasePath + "/restart", new StringContent(""), false);
}