using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeAicService : IAicService
{
    public Task<Result<bool?>> IsAvailable()
    {
        return Task.FromResult(Result<bool?>.Ok(true));
    }

    public Task<Result> IsOnline()
    {
        return Task.FromResult(Result.Ok());
    }

    public Task<Result<string>> Version()
    {
        return Task.FromResult(Result<string>.Ok("DebugVersion"));
    }

    public Task<Result<string[]>> LogDates()
    {
        return Task.FromResult(Result<string[]>.Ok(
        [
            "04-09-2016",
            "01-01-2004"
        ]));
    }

    public Task<Result<string[]>> Logs(string date)
    {
        return Task.FromResult(Result<string[]>.Ok(
        [
            "Example Log.",
            "Another example log..."
        ]));
    }

    public Task<Result> Shutdown()
    {
        return Task.FromResult(Result.Ok());
    }

    public Task<Result> Restart()
    {
        return Task.FromResult(Result.Ok());
    }
}