using AutoTf.TabletOS.Avalonia.ViewModels;
using Avalonia.Controls;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		Statics.Window = this;
		DataContext = new MainWindowViewModel();
		InitializeComponent();
	}
}