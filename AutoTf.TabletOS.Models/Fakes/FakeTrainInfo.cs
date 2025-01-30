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
		return Task.FromResult(DateTime.Now.ToString("o"))!;
	}

	public Task<string?> GetVersion()
	{
		return Task.FromResult("9ahf8v")!;
	}

	public Task<DateTime?> GetNextSave()
	{
		return Task.FromResult(new DateTime?(DateTime.Now));
	}

	public Task<bool> PostUpdate()
	{
		return Task.FromResult(true);
	}

	public Task<bool> PostShutdown()
	{
		return Task.FromResult(true);
	}

	public Task<bool> PostRestart()
	{
		return Task.FromResult(true);
	}
}