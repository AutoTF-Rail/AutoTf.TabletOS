using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
}