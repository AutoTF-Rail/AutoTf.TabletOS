using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models.Fakes;

public class FakeRcInteractions : IRcInteractions
{
	public void StartMonitoring()
	{
	}

	public void StopMonitoring()
	{
	}

	public string ReadCardContent(byte[]? key = null)
	{

		return "Fake";
	}

	public event EventHandler<string>? CardInserted;
	public event EventHandler<string>? CardRemoved;
	public event EventHandler<string>? CardRead;
}