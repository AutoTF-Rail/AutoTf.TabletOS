namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainInformationService
{
	public Task<string?> GetEvuName();
	public Task<string?> GetTrainId();
	public Task<string?> GetTrainName();
	public Task<string?> GetLastSync();
}