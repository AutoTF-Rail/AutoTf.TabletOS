using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Services.Fakes;

/// <summary>
/// Fake datamanager that is automatically used when app is not run in release
/// </summary>
public class FakeDataManager : IDataManager
{
	public DateTime GetLastSynced() => DateTime.Now.Subtract(TimeSpan.FromDays(1));
	public void SaveLastSynced(DateTime time)
	{
	}
}