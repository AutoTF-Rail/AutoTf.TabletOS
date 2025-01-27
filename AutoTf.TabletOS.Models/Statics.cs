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
}