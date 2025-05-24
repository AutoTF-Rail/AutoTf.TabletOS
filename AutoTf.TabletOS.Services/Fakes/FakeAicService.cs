using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeAicService : IAicService
{
    public Task<bool?> IsAvailable()
    {
        return Task.FromResult<bool?>(true);
    }

    public Task<bool> IsOnline()
    {
        return Task.FromResult(true);
    }

    public Task<string?> Version()
    {
        return Task.FromResult("DebugVersion")!;
    }

    public Task<string[]> LogDates()
    {
        return Task.FromResult(new[]
        {
            "04-09-2016",
            "01-01-2004"
        });
    }

    public Task<string[]> Logs(string date)
    {
        return Task.FromResult(new[]
        {
            "Example Log.",
            "Another example log..."
        });
    }

    public void Shutdown()
    {
        
    }

    public void Restart()
    {
        
    }
}