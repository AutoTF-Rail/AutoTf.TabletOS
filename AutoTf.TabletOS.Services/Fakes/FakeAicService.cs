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

    public void Shutdown()
    {
        
    }

    public void Restart()
    {
        
    }
}