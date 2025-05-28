using AutoTf.CentralBridge.Shared.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeTrainInformationService : ITrainInformationService
{
	public Task<Result<string>> GetEvuName()
	{
		return Task.FromResult(Result<string>.Ok("ExampleEvu2"));
	}

	public Task<Result<string>> GetTrainId()
	{
		return Task.FromResult(Result<string>.Ok("836-378"));
	}

	public Task<Result<string>> GetTrainName()
	{
		return Task.FromResult(Result<string>.Ok("Desiro HC"));
	}

	public Task<Result<DateTime>> GetLastSync()
	{
		return Task.FromResult(Result<DateTime>.Ok(DateTime.Now));
	}

	public Task<Result<string>> GetVersion()
	{
		return Task.FromResult(Result<string>.Ok("9ahf8v"));
	}

	public Task<Result<DateTime>> GetNextSave()
	{
		return Task.FromResult(Result<DateTime>.Ok(DateTime.Now));
	}
	
	public Task<Result<string[]>> GetLogDates()
	{
		return Task.FromResult(Result<string[]>.Ok(
		[
			"22.02.2024",
			"22.02.2024",
			"22.02.2024",
			"22.02.2024",
		]));
	}

	public Task<Result<string[]>> GetLogs(string date)
	{
		List<string> logs = new List<string>();
		for (int i = 0; i < 50; i++)
		{
			logs.Add("TestLog");
		}
		logs.Add(string.Join(",", logs));
		logs.Add("Final");
		return Task.FromResult(Result<string[]>.Ok(logs.ToArray()));
	}

	public Task<Result> PostUpdate()
	{
		return Task.FromResult(Result.Ok());
	}

	public Task<Result> PostShutdown()
	{
		return Task.FromResult(Result.Ok());
	}

	public Task<Result> PostRestart()
	{
		return Task.FromResult(Result.Ok());
	}

	public Task<Result> SetDate(DateTime date)
	{
		return Task.FromResult(Result.Ok());
	}
}