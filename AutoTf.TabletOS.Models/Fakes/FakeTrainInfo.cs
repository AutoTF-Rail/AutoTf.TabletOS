using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models.Fakes;

public class FakeTrainInfo : ITrainInformationService
{
	public Task<int?> GetCameraCount()
	{
		return Task.FromResult<int?>(null);
	}

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

	public Task<bool> PostStartStream(int port, int cameraIndex)
	{
		return Task.FromResult<bool>(true);
	}

	public Task<bool> PostStartStream()
	{
		return Task.FromResult(true);
	}

	public Task<bool> PostStopStream()
	{
		return Task.FromResult(true);
	}

	public Task<string[]?> GetLogDates()
	{
		return Task.FromResult(new[]
		{
			"22.02.2024",
			"22.02.2024",
			"22.02.2024",
			"22.02.2024",
		})!;
	}

	public Task<string[]?> GetLogs(string date)
	{
		return Task.FromResult(new[]
		{
			"22.02.2024",
			"22.02.2024",
			"22.02.2024",
			"22.02.2024",
		})!;
	}
}