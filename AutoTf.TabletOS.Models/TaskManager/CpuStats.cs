namespace AutoTf.TabletOS.Models.TaskManager;

public class CpuStats
{
	public CpuStats(float total, float idle)
	{
		Total = total;
		Idle = idle;
	}
	
	public float Total { get; }
	public float Idle { get; }
}