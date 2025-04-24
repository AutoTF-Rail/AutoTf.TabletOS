using AutoTf.TabletOS.Avalonia.Views;
using AutoTf.TabletOS.Models;
using Avalonia.Controls;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
	private object _activeView = null!;

	public object ActiveView
	{
		get => _activeView;
		private set => this.RaiseAndSetIfChanged(ref _activeView, value);
	}

	public MainWindowViewModel()
	{
		ActiveView = new MainView();
		Statics.ChangeViewModel += ChangeView;
	}

	private void ChangeView(UserControl obj)
	{
		// if(obj is CallBackUserControl ctrl)
		// 	ctrl.Callback();
		
		ActiveView = obj;
	}
}