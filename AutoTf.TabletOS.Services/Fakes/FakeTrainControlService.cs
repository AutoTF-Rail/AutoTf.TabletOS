using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeTrainControlService : ITrainControlService
{
	private Dictionary<int, double> _leverPositions = new Dictionary<int, double>()
	{
		{ 0, 0 },
		{ 1, -100 }
	};
	
	public Task<Result<int>> GetLeverCount()
	{
		return Task.FromResult(Result<int>.Ok(1));
	}

	public Task<Result<double>> GetLeverPosition(int leverIndex)
	{
		return Task.FromResult(Result<double>.Ok(_leverPositions[leverIndex]));
	}

	public Task<Result<LeverType>> GetLeverType(int leverIndex)
	{
		if (leverIndex == 0)
			return Task.FromResult(Result<LeverType>.Ok(LeverType.CombinedThrottle));
		else
			return Task.FromResult(Result<LeverType>.Ok(LeverType.MainBrake));
	}

	public Task<Result> SetLever(LeverSetModel leverModel)
	{
		_leverPositions[leverModel.LeverIndex] = leverModel.Percentage;
		return Task.FromResult(Result.Ok());
	}

	public Task<Result<bool>> IsEasyControlAvailable()
	{
		return Task.FromResult(Result<bool>.Ok(true));
	}

	public Task<Result> EasyControl(int power)
	{
		return Task.FromResult(Result.Ok());
	}

	public Task<Result> EmergencyBrake()
	{
		return Task.FromResult(Result.Ok());
	}
}