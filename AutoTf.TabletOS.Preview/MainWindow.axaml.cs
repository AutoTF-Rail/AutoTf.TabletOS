using AutoTf.TabletOS.Avalonia.Views;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Preview;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		DataContext = new MainWindowViewModel();
		InitializeComponent();
	}
}