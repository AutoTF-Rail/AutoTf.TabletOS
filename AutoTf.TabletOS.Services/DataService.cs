using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services;

public class DataService : IDataManager
{
	private readonly string _directory = "/etc/AutoTf/TabletOS";
	
	public DataService()
	{
		Directory.CreateDirectory(_directory);
	}
	
	public bool ReadFile(string fileName, out string content)
	{
		content = "";
		string path = Path.Combine(_directory, fileName);
		if (!File.Exists(path))
			return false;

		content = File.ReadAllText(path);
		return true;
	}
	
	public string ReadFile(string fileName, string replacement = "")
	{
		string path = Path.Combine(_directory, fileName);

		if (!File.Exists(path))
		{
			File.WriteAllText(path, replacement);
			return replacement;
		}

		return File.ReadAllText(path);
	}

	// We don't need to create the file here afterwards, because we didn't sync yet.
	public DateTime GetLastSynced() => !File.Exists(Path.Combine(_directory, "LastSynced")) ? DateTime.MinValue : DateTime.Parse(File.ReadAllText(Path.Combine(_directory, "LastSynced")));

	public void SaveLastSynced(DateTime time)
	{
		File.WriteAllText(Path.Combine(_directory, "LastSynced"), time.ToString("MM/dd/yyyyTHH:mm:ss"));
	}
}