using System.Diagnostics;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Models;

public static class Statics
{
	public static Action? BrightnessChanged;
	public static double CurrentBrightness = 1.0;
	public static IDataManager DataManager;
	public static IProcessReader ProcessReader;
	public static ITrainInformationService TrainInformationService;
	
	public static NetworkManager NetworkManager = new NetworkManager();
	
	public static Window Window;
	public static string Username { get; set; }
	public static string Password { get; set; }

	public static ConnectionType Connection { get; set; } = ConnectionType.None;
	
	
	public static string YubiCode { get; set; }
	public static int YubiSerial { get; set; }
	public static DateTime YubiTime { get; set; }
	// TODO: Tablet gets a key from trains
	
	public static string GenerateRandomString()
	{
		Random random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 10)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
	
	public static string ExecuteCommand(string command)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			}
		};

		process.Start();
		string result = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		return result;
	}
}