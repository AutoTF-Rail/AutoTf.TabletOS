using System.Net.NetworkInformation;
using AutoTf.Logging;

namespace AutoTf.TabletOS.Models;

public class NetworkManager
{
	private readonly Logger _logger = Statics.Logger;
	
	public NetworkManager()
	{
		Statics.Shutdown += ShutdownConnection;
	}
	
	public static bool IsInternetAvailable()
	{
		try
		{
			using Ping ping = new Ping();

			PingReply reply = ping.Send("1.1.1.1", 1500);
			if (reply.Status == IPStatus.Success)
			{
				return true;
			}
		}
		catch
		{
			// ignored
		}

		return false;
	}

	public void ShutdownConnection()
	{
		if(Statics.TrainConnectionId != null)
			CommandExecuter.ExecuteSilent("nmcli connection delete CentralBridge-" + Statics.TrainConnectionId, true);

		Statics.Connection = ConnectionType.None;
	}
	
	public string? EstablishConnection(string name, bool isTrain)
	{
		_logger.Log("Establishing connection via connection ID: " + Statics.TrainConnectionId);
		CommandExecuter.ExecuteSilent($"nmcli c add type wifi con-name CentralBridge-{Statics.TrainConnectionId} ifname wlan0 ssid {name}", true);
		CommandExecuter.ExecuteSilent($"nmcli con modify CentralBridge-{Statics.TrainConnectionId} wifi-sec.key-mgmt wpa-psk", true);
		CommandExecuter.ExecuteSilent($"nmcli con modify CentralBridge-{Statics.TrainConnectionId} wifi-sec.psk CentralBridgePW", true);
		CommandExecuter.ExecuteSilent($"nmcli con modify CentralBridge-{Statics.TrainConnectionId} connection.autoconnect no", true);
		
		string output = CommandExecuter.ExecuteCommand($"nmcli con up CentralBridge-{Statics.TrainConnectionId}");

		if (!output.Contains("Connection successfully activated"))
		{
			if (isTrain)
				Statics.Connection = ConnectionType.Train;
			else
				Statics.Connection = ConnectionType.Mesh;

			return null;
		}

		return output;
		// TODO: Do we even need to sync anything?
		// Updates?
		// Statics.DataManager.SaveLastSynced(DateTime.Now);
	}

	public void ScanForMesh()
	{
		// string scanResult = ExecuteBashCommand("sudo iwlist wlan0 scan");

		// Console.WriteLine("Available Networks:");
		// Console.WriteLine(scanResult);
		// If mesh, login with Default password (meshes use the mac allow list too)
	}
}