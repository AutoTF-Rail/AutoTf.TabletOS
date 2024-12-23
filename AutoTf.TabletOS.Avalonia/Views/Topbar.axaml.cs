using System;
using System.Diagnostics;
using System.Globalization;
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
		
		StaticEvents.BrightnessChanged += BrightnessChanged;
	}

	private void BrightnessChanged()
	{
		// this.Opacity = StaticEvents.CurrentBrightness;
	}

	private void Initialize()
	{
		// QuickMenuGrid.IsVisible = false;

		VersionBox.Text = "Version: " + Program.GetGitVersion();
		
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1) 
		};
		_timer.Tick += UpdateClock; 
		_timer.Start(); 

		UpdateClock(null, null);
	}
	
	private async void UpdateClock(object? sender, EventArgs e)
	{
		Bluber.Text = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
		if (QuickMenuGrid.IsVisible)
		{
			CpuUsage.Text = float.Round((await Program.GetCpuUsageAsync()), MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture) + "%";
			RamUsage.Text = float.Round(Program.GetUsedMemory()) + "MB/" + float.Round(Program.GetTotalMemory()) + "MB";
		}
	}

	private void ToggleQuickMenu(object? sender, PointerReleasedEventArgs e)
	{
		QuickMenuGrid.IsVisible = !QuickMenuGrid.IsVisible;
	}

	private async void Shutdown_Click(object? sender, RoutedEventArgs e)
	{
		Popup popup = new Popup("Are you sure you want to shutdown?");
		if (await popup.Show(RootGrid) != DialogResult.Yes)
			return;
		
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

	private void DarkerButton_Click(object? sender, RoutedEventArgs e)
	{
		if (StaticEvents.CurrentBrightness <= .4)
			return;
		StaticEvents.CurrentBrightness -= .1f;
		StaticEvents.BrightnessChanged?.Invoke();
	}

	private void BrighterButton_Click(object? sender, RoutedEventArgs e)
	{
		if (StaticEvents.CurrentBrightness >= 1.0f)
			return;
		StaticEvents.CurrentBrightness += .1f;
		StaticEvents.BrightnessChanged?.Invoke();
	}
}