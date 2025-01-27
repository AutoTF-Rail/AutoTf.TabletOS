using System.Diagnostics;
using System.Net.NetworkInformation;

namespace AutoTf.TabletOS.Models;

public class NetworkManager
{
	public bool IsInternetAvailable()
	{
		try
		{
			using var ping = new Ping();

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

	public void EstablishConnection()
	{

		
		// TODO: Do we even need to sync anything?
		// Updates?
		Statics.DataManager.SaveLastSynced(DateTime.Now);
	}

	public void ScanForMesh()
	{
		// string scanResult = ExecuteBashCommand("sudo iwlist wlan0 scan");

		// Console.WriteLine("Available Networks:");
		// Console.WriteLine(scanResult);
		// If mesh, login with Default password (meshes use the mac allow list too)
	}
	
	private string ExecuteCommand(string command)
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