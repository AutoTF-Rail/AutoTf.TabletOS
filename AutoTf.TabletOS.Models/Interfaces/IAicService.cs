using Avalonia.Media;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface IAicService
{
    public Task<bool?> IsAvailable();
    public Task<bool> IsOnline();
    public Task<string?> Version();
    public void Shutdown();
    public void Restart();
}