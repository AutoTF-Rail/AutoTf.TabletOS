using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public static class Statics
{
	public static string? TrainConnectionId;
	
	// TODO: Implement global dispose etc
	public static Action? Shutdown;
	public static IDataManager DataManager = null!;
	public static IProcessReader ProcessReader = null!;
	public static ITrainInformationService TrainInformationService = null!;
	public static ITrainControlService TrainControlService = null!;
	public static ITrainCameraService TrainCameraService = null!;
	
	public static Logger Logger = new Logger(true);
	
	public static NetworkManager NetworkManager = new NetworkManager();
	public static ConnectionType Connection { get; set; } = ConnectionType.None;
	
	public static string YubiCode { get; set; } = null!;
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
}