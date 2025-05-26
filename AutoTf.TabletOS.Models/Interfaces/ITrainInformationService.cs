using AutoTf.CentralBridge.Shared.Models;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainInformationService
{
	public Task<Result<string>> GetEvuName();
	public Task<Result<string>> GetTrainId();
	public Task<Result<string>> GetTrainName();
	public Task<Result<string>> GetLastSync();
	public Task<Result<string>> GetVersion();
	public Task<Result<DateTime>> GetNextSave();
	public Task<Result<string[]>> GetLogDates();
	public Task<Result<string[]>> GetLogs(string date);
	public Task<Result> PostUpdate();
	public Task<Result> PostShutdown();
	public Task<Result> PostRestart();
	public Task<Result> SetDate(DateTime date);
}