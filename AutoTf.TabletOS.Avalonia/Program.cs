using System;
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
}