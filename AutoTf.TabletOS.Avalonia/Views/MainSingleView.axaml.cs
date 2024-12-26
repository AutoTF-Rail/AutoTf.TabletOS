using AutoTf.TabletOS.Avalonia.ViewModels;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainSingleView : UserControl
{
	public MainSingleView()
	{
		DataContext = new MainWindowViewModel();
		InitializeComponent();
	}
}