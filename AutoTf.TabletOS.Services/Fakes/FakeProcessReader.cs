using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services.Fakes;

public class FakeProcessReader : IProcessReader
{
	public float GetTotalMemory() => 0.0f;

	public float GetUsedMemory() => 0.0f;

	public float GetCpuTemperature() => 0.0f;

	public async Task<float> GetCpuUsageAsync() => await Task.FromResult(0.0f);

	public CpuStats? ReadCpuStats() => new CpuStats(0.0f, 0.0f);
}