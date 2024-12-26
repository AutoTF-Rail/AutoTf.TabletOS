namespace AutoTf.TabletOS.Models.Interfaces;

public interface IProcessReader
{
	public float GetTotalMemory();
	public float GetUsedMemory();
	public float GetCpuTemperature();
	public Task<float> GetCpuUsageAsync();
	public CpuStats? ReadCpuStats();
}