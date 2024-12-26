using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public class DataManager : IDataManager
{
	private readonly string _directory = "/etc/AutoTf/TabletOS";
	
	public DataManager()
	{
		Directory.CreateDirectory(_directory);
	}
	
	// We don't need to create the file here afterwards, because we didn't sync yet.
	public string GetLastSynced() => !File.Exists(Path.Combine(_directory, "LastSynced")) ? "Unknown" : File.ReadAllText(Path.Combine(_directory, "LastSynced"));
}