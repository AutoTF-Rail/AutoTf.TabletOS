using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models.Enums;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface ITrainControlService
{
	public Task<Result<bool>> IsEasyControlAvailable();
	public Task<Result<int>> GetLeverCount();
	public Task<Result<double>> GetLeverPosition(int leverIndex);
	public Task<Result<LeverType>> GetLeverType(int leverIndex);
	public Task<Result> SetLever(LeverSetModel leverModel);
	public Task<Result> EasyControl(int power);
	public Task<Result> EmergencyBrake();
}