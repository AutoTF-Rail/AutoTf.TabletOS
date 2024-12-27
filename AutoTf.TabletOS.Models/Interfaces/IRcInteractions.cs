namespace AutoTf.TabletOS.Models.Interfaces;

public interface IRcInteractions
{
	public void StartMonitoring();
	public void StopMonitoring();
	public string ReadCardContent(byte[]? key = null);
	public bool WriteToCard(byte[] data, byte blockNumber);
	
	public event EventHandler<string> CardInserted;
	public event EventHandler<string> CardRemoved;
	public event EventHandler<string> CardRead;
}