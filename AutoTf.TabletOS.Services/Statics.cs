using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AutoTf.Logging;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public static class Statics
{
	private static string? _macAddress;
	
	static Statics()
	{
		Notifications.CollectionChanged += NotificationsOnCollectionChanged;
	}
	
	public static Action? Shutdown;
	public static ConnectionType Connection { get; set; } = ConnectionType.None;
	
	public static ITrainInformationService TrainInformationService = null!;
	public static ITrainControlService TrainControlService = null!;
	public static ITrainCameraService TrainCameraService = null!;
	public static IDataManager DataManager = null!;
	public static IProcessReader ProcessReader = null!;

	public static NetworkService NetworkService = new NetworkService();
	public static Logger Logger = new Logger(true);
	
	public static string YubiCode { get; set; } = null!;
	public static int YubiSerial { get; set; }
	public static DateTime YubiTime { get; set; }
	
	public static string? TrainConnectionId;
	
	public static ObservableCollection<Notification> Notifications { get; set; } = new ObservableCollection<Notification>();
	
	public static string MacAddress
	{
		get
		{
			if (_macAddress == null)
				_macAddress = CommandExecuter.ExecuteCommand("cat /sys/class/net/wlan0/address").TrimEnd();

			return _macAddress;
		}
	}

	private static void NotificationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action != NotifyCollectionChangedAction.Add) return;
		foreach (object? newItem in e.NewItems!)
		{
			Logger.Log($"New Notification: {((Notification)newItem).Text}");
		}
	}
	
	public static string GenerateRandomString()
	{
		Random random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 10)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
}