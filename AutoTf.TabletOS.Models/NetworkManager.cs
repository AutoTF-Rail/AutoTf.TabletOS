using System.Net.NetworkInformation;

namespace AutoTf.TabletOS.Models;

public static class NetworkManager
{
	public static bool IsInternetAvailable()
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
}