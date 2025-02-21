using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AutoTf.TabletOS.Services;
using Avalonia;
using Statics = AutoTf.TabletOS.Services.Statics;

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
		try
		{
			AppBuilder builder = BuildAvaloniaApp();
			Initialize();
			if (args.Contains("--drm"))
			{
				SilenceConsole();
				// By default, Avalonia will try to detect output card automatically.
				// But you can specify one, for example "/dev/dri/card1".
				return builder.StartLinuxDrm(args: args, card: "/dev/dri/card1", scaling: 1.0);
			}

			return builder.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception e)
		{
			Console.WriteLine("Root error:");
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			Statics.Shutdown?.Invoke();
		}

		return -1;
	}

	private static void Initialize()
	{
#if RELEASE
		Statics.DataManager = new DataService();
		Statics.TrainInformationService = new TrainInformationService();
		Statics.TrainControlService = new TrainControlService();
		Statics.TrainCameraService = new TrainCameraService();
#else
		Statics.DataManager = new FakeDataManager();
		Statics.TrainInformationService = new FakeTrainInfo();
		Statics.TrainControlService = new FakeTrainControlService();
		Statics.TrainCameraService = new FakeTrainCameraService();
#endif
	}

	private static void SilenceConsole()
	{
		if (!IsInteractiveEnvironment())
		{
			return;
		}
		
		new Thread(() =>
			{
				Console.CursorVisible = false;
				while (true)
					Console.ReadKey(true);
			})
			{ IsBackground = true }.Start();
	}
	
	private static bool IsInteractiveEnvironment()
	{
		return Environment.UserInteractive &&
		       !Console.IsOutputRedirected &&
		       !Console.IsErrorRedirected;
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