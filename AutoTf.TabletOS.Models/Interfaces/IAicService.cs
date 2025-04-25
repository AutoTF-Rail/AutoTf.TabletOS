using Avalonia.Media;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface IAicService
{
    public Task<bool?> IsAvailable();
    public Task<bool> IsOnline();
    public Task<string?> Version();
    public Task<string[]> LogDates();
    public Task<string[]> Logs(string date);
    public void Shutdown();
    public void Restart();
}