using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
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
		
		Statics.BrightnessChanged += BrightnessChanged;
	}

	private void BrightnessChanged()
	{
		// this.Opacity = StaticEvents.CurrentBrightness;
	}

	public void Initialize()
	{
		// QuickMenuGrid.IsVisible = false;

		// LastSynced.Text = "Last Synced: " + Statics.DataManager.GetLastSynced();

		
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1) 
		};
		_timer.Tick += UpdateClock; 
		_timer.Start();
		byte[] blockData = new byte[16];
		Array.Copy(Encoding.UTF8.GetBytes("MeowMeowMeow"), blockData, Math.Min(Encoding.UTF8.GetBytes("MeowMeowMeow").Length, blockData.Length));
		Statics.RcInteraction.WriteToCard(blockData, 5);

		UpdateClock(null, null);
	}
	private async void UpdateClock(object? sender, EventArgs e)
	{
		Bluber.Text = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
		if (QuickMenuGrid.IsVisible)
		{
			CpuUsage.Text = float.Round((await Statics.ProcessReader.GetCpuUsageAsync()), MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture) + "%";
			RamUsage.Text = float.Round(Statics.ProcessReader.GetUsedMemory()) + "MB/" + float.Round(Statics.ProcessReader.GetTotalMemory()) + "MB";
			RfidStatus.Text = Statics.RcInteraction.ReadCardContent();
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
		if (Statics.CurrentBrightness <= .4)
			return;
		Statics.CurrentBrightness -= .1f;
		Statics.BrightnessChanged?.Invoke();
	}

	private void BrighterButton_Click(object? sender, RoutedEventArgs e)
	{
		if (Statics.CurrentBrightness >= 1.0f)
			return;
		Statics.CurrentBrightness += .1f;
		Statics.BrightnessChanged?.Invoke();
	}

	private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
	{
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = new InfoScreen(viewModel.ActiveView);
		}
		// else ErrorBox.Text = "No Context";
	}
}