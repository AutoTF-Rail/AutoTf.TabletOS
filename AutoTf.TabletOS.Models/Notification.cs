using Avalonia.Media;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Models;

public class Notification
{
	public Notification(string text, Color color)
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			Text = text;
			Time = DateTime.Now.ToString("HH:mm:ss");
			Color = new SolidColorBrush(color);
		});
	}
	
	public string Text { get; set; }
	public string Time { get; set; }
	public SolidColorBrush Color { get; set; }
}