using AutoTf.TabletOS.Models.Enums;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainControlService
{
	public Task<int> GetLeverCount();
	public Task<double> GetLeverPosition(int leverIndex);
	public Task<LeverType> GetLeverType(int leverIndex);
	public Task<bool> SetLever(int leverIndex, double leverPercentage);
}