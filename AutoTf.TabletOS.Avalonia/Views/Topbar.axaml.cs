using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TopBar : UserControl
{
	private DispatcherTimer _timer;
	
	public TopBar()
	{
		InitializeComponent();
		Initialize();
	}

	private void Initialize()
	{
		// QuickMenuGrid.IsVisible = false;
		
		
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1) 
		};
		_timer.Tick += UpdateClock; 
		_timer.Start(); 

		UpdateClock(null, null);
	}
	
	private void UpdateClock(object? sender, EventArgs e)
	{
		Bluber.Text = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
	}

	private void ToggleQuickMenu(object? sender, PointerReleasedEventArgs e)
	{
		QuickMenuGrid.IsVisible = !QuickMenuGrid.IsVisible;
	}

	private void Shutdown_Click(object? sender, RoutedEventArgs e)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "shutdown",
				Arguments = "now",
				CreateNoWindow = true,
				UseShellExecute = false
			}
		};

		process.Start();
		Environment.Exit(0);
	}
}