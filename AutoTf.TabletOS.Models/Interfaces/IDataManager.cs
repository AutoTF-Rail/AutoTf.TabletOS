namespace AutoTf.TabletOS.Models.Interfaces;

public interface IDataManager
{
	public DateTime GetLastSynced();
	public void SaveLastSynced(DateTime time);
}