using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		Statics.Window = this;
		InitializeComponent();
	}
}