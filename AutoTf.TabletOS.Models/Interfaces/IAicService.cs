using AutoTf.CentralBridge.Shared.Models;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface IAicService
{
    public Task<Result<bool?>> IsAvailable();
    public Task<Result> IsOnline();
    public Task<Result<string>> Version();
    public Task<Result<string[]>> LogDates();
    public Task<Result<string[]>> Logs(string date);
    public Task<Result> Shutdown();
    public Task<Result> Restart();
}