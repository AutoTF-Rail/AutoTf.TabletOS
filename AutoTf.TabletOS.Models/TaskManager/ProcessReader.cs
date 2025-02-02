using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models.TaskManager;

public class ProcessReader : IProcessReader
{
	private readonly Logger _logger = Statics.Logger;
	
	public float GetTotalMemory()
	{
		try
		{
			string[] lines = File.ReadAllLines("/proc/meminfo");
			string? totalLine = lines.FirstOrDefault(line => line.StartsWith("MemTotal"));
			if (totalLine != null)
			{
				string[] parts = totalLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (float.TryParse(parts[1], out float totalKb))
				{
					return totalKb / 1024;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.Log($"Error reading total memory: {ex.Message}");
		}
		return -1;
	}

	public float GetUsedMemory()
	{
		try
		{
			string[] lines = File.ReadAllLines("/proc/meminfo");
			string? totalLine = lines.FirstOrDefault(line => line.StartsWith("MemTotal"));
			string? freeLine = lines.FirstOrDefault(line => line.StartsWith("MemAvailable"));
			if (totalLine != null && freeLine != null)
			{
				string[] totalParts = totalLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				string[] freeParts = freeLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if (float.TryParse(totalParts[1], out float totalKb) && float.TryParse(freeParts[1], out float freeKb))
				{
					return (totalKb - freeKb) / 1024;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.Log($"Error reading used memory: {ex.Message}");
		}
		return -1;
	}

	public float GetCpuTemperature()
	{
		try
		{
			string tempString = File.ReadAllText("/sys/class/thermal/thermal_zone0/temp");
			if (float.TryParse(tempString, out float temperature))
			{
				return temperature / 1000.0f; 
			}
		}
		catch (Exception ex)
		{
			_logger.Log($"Error reading CPU temperature: {ex.Message}");
		}
		return -1;
	}

	public async Task<float> GetCpuUsageAsync()
	{
		try
		{
			CpuStats? initialCpuStats = ReadCpuStats();
			if (initialCpuStats == null) return -1;

			await Task.Delay(1000);

			CpuStats? finalCpuStats = ReadCpuStats();
			if (finalCpuStats == null) return -1;

			float totalDelta = finalCpuStats.Total - initialCpuStats.Total;
			float idleDelta = finalCpuStats.Idle - initialCpuStats.Idle;

			return (1 - idleDelta / totalDelta) * 100;
		}
		catch (Exception ex)
		{
			_logger.Log($"Error calculating CPU usage: {ex.Message}");
			return -1;
		}
	}

	public CpuStats? ReadCpuStats()
	{
		try
		{
			string? cpuLine = File.ReadLines("/proc/stat").FirstOrDefault(line => line.StartsWith("cpu "));
			if (cpuLine != null)
			{
				float[] parts = cpuLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1)
					.Select(float.Parse).ToArray();
				float total = parts.Sum();
				float idle = parts[3];
				return new CpuStats(total, idle);
			}
		}
		catch (Exception ex)
		{
			_logger.Log($"Error reading CPU stats: {ex.Message}");
		}
		return null;
	}
}