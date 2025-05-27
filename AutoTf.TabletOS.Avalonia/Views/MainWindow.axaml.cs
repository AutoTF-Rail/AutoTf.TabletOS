using AutoTf.Logging;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainWindow : Window
{
	public MainWindow(Logger logger)
	{
		logger.Log("Starting up");
		InitializeComponent();
	}
	
	public void SetView(UserControl view)
	{
		ActiveView.Content = view;
	}
    
	public void ShowLoadingScreen(bool visible, string text = "")
	{
		LoadingName.Text = text;
		LoadingArea.IsVisible = visible;
	}
}