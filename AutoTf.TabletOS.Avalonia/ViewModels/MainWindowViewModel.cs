using AutoTf.TabletOS.Avalonia.Views;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels;

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
		ActiveView = new MainView();
	}
}