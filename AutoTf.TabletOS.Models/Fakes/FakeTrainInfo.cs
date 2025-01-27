using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models.Fakes;

public class FakeTrainInfo : ITrainInformationService
{
	public Task<string?> GetEvuName()
	{
		return Task.FromResult("ExampleEvu2")!;
	}

	public Task<string?> GetTrainId()
	{
		return Task.FromResult("836-378")!;
	}

	public Task<string?> GetTrainName()
	{
		return Task.FromResult("Desiro Meow")!;
	}

	public Task<string?> GetLastSync()
	{
		return Task.FromResult(DateTime.Now.ToString())!;
	}
}