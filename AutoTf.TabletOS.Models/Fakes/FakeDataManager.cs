using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

/// <summary>
/// Fake datamanager that is automatically used when app is not run in release
/// </summary>
public class FakeDataManager : IDataManager
{
	public string GetLastSynced() => "Unknown";
}