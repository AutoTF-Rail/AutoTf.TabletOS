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
	
	public Task<int> GetLeverCount()
	{
		return Task.FromResult(1);
	}

	public Task<double> GetLeverPosition(int leverIndex)
	{
		return Task.FromResult(_leverPositions[leverIndex]);
	}

	public Task<LeverType> GetLeverType(int leverIndex)
	{
		if (leverIndex == 0)
			return Task.FromResult(LeverType.CombinedThrottle);
		else
			return Task.FromResult(LeverType.MainBrake);
	}

	public Task<bool> SetLever(LeverSetModel leverModel)
	{
		_leverPositions[leverModel.LeverIndex] = leverModel.Percentage;
		return Task.FromResult(true);
	}

	public Task<bool> IsEasyControlAvailable()
	{
		return Task.FromResult(true);
	}

	public Task<bool> EasyControl(int power)
	{
		return Task.FromResult(true);
	}

	public Task<bool> EmergencyBrake()
	{
		return Task.FromResult(true);
	}
}