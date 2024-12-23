using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace AutoTf.TabletOS.Avalonia;

sealed class Program
{
	// // Initialization code. Don't use any Avalonia, third-party APIs or any
	// // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// // yet and stuff might break.
	// [STAThread]
	// public static void Main(string[] args) => BuildAvaloniaApp()
	// 	.StartWithClassicDesktopLifetime(args);
	//
	// // Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
	
	public static int Main(string[] args)
	{
		AppBuilder builder = BuildAvaloniaApp();
		if (args.Contains("--drm"))
		{
			SilenceConsole();
			// By default, Avalonia will try to detect output card automatically.
			// But you can specify one, for example "/dev/dri/card1".
			return builder.StartLinuxDrm(args: args, card: "/dev/dri/card1", scaling: 1.0);
		}

		return builder.StartWithClassicDesktopLifetime(args);
	}

	private static void SilenceConsole()
	{
		new Thread(() =>
			{
				Console.CursorVisible = false;
				while (true)
					Console.ReadKey(true);
			})
			{ IsBackground = true }.Start();
	}
	
	public static string GetGitVersion()
	{
		try
		{
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "git",
				Arguments = "describe --tags --always",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			using (Process process = new Process { StartInfo = psi })
			{
				process.Start();
				string output = process.StandardOutput.ReadToEnd().Trim();
				string error = process.StandardError.ReadToEnd().Trim();

				process.WaitForExit();

				if (!string.IsNullOrWhiteSpace(error))
				{
					throw new Exception($"Git Error: {error}");
				}

				return output;
			}
		}
		catch (Exception ex)
		{
			return $"Error retrieving Git version: {ex.Message}";
		}
	}
	
	public static float GetTotalMemory()
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
			Console.WriteLine($"Error reading total memory: {ex.Message}");
		}
		return -1;
	}

	public static float GetUsedMemory()
	{
		try
		{
			var lines = File.ReadAllLines("/proc/meminfo");
			var totalLine = lines.FirstOrDefault(line => line.StartsWith("MemTotal"));
			var freeLine = lines.FirstOrDefault(line => line.StartsWith("MemAvailable"));
			if (totalLine != null && freeLine != null)
			{
				var totalParts = totalLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				var freeParts = freeLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if (float.TryParse(totalParts[1], out var totalKb) && float.TryParse(freeParts[1], out var freeKb))
				{
					return (totalKb - freeKb) / 1024;
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error reading used memory: {ex.Message}");
		}
		return -1;
	}

	public static float GetCpuTemperature()
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
			Console.WriteLine($"Error reading CPU temperature: {ex.Message}");
		}
		return -1;
	}

	public static async Task<float> GetCpuUsageAsync()
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
			Console.WriteLine($"Error calculating CPU usage: {ex.Message}");
			return -1;
		}
	}

	public static CpuStats? ReadCpuStats()
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
			Console.WriteLine($"Error reading CPU stats: {ex.Message}");
		}
		return null;
	}
}
class CpuStats
{
	public float Total { get; }
	public float Idle { get; }

	public CpuStats(float total, float idle)
	{
		Total = total;
		Idle = idle;
	}
}