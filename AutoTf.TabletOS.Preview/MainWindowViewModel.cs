using AutoTf.TabletOS.Avalonia.Views;
using ReactiveUI;

namespace AutoTf.TabletOS.Preview;

public partial class MainWindowViewModel : ReactiveObject
{
	private object _activeView = null!;

	public object ActiveView
	{
		get => _activeView;
		set => this.RaiseAndSetIfChanged(ref _activeView, value);
	}

	public MainWindowViewModel()
	{
		ActiveView = new TrainControlView();
		// Statics.Notifications.Add(new Notification("Meow", Colors.Red));
		// Statics.Notifications.Add(new Notification("grrr", Colors.Yellow));
		// Statics.Notifications.Add(new Notification("buh", Colors.Green));
	}
}