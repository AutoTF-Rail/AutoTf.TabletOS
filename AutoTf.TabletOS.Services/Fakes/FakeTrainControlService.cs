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
		return Task.FromResult(0);
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

	public Task<bool> SetLever(int leverIndex, double leverPercentage)
	{
		_leverPositions[leverIndex] = leverPercentage;
		return Task.FromResult(true);
	}
}