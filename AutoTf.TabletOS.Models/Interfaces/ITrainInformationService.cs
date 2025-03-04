namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainInformationService
{
	public Task<string?> GetEvuName();
	public Task<string?> GetTrainId();
	public Task<string?> GetTrainName();
	public Task<string?> GetLastSync();
	public Task<string?> GetVersion();
	public Task<DateTime?> GetNextSave();
	public Task<bool> PostUpdate();
	public Task<bool> PostShutdown();
	public Task<bool> PostRestart();
	public Task<string[]?> GetLogDates();
	public Task<string[]?> GetLogs(string date);
	public Task<bool> SetDate(DateTime date);
}