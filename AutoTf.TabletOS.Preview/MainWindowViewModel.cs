using AutoTf.TabletOS.Avalonia.Views;
using Avalonia.Controls;
using ReactiveUI;

namespace AutoTf.TabletOS.Preview;

public partial class MainWindowViewModel : ReactiveObject
{
	private object _activeView;

	public object ActiveView
	{
		get => _activeView;
		set => this.RaiseAndSetIfChanged(ref _activeView, value);
	}

	public MainWindowViewModel()
	{
		ActiveView = new LogsViewer();
	}
}