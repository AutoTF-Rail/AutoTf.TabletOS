using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();

		Statics.BrightnessChanged += BrightnessChanged;
		LoadingArea.IsVisible = false;

		Task.Run(ListenForYubikey);
	}

	private void ListenForYubikey()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo
		{
			FileName = "udevadm",
			Arguments = "monitor --subsystem-match=usb --property",
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		Process process = new Process { StartInfo = processStartInfo };
		process.OutputDataReceived += (sender, e) =>
		{
			if (e.Data != null)
			{
				if (e.Data.Contains("Yubico") || e.Data.Contains("YubiKey"))
				{
					Dispatcher.UIThread.Invoke(() =>
					{	
						LoadingName.Text = "";
						LoadingArea.IsVisible = true;
						
						if (DataContext is MainWindowViewModel viewModel)
						{
							viewModel.ActiveView = new InfoScreen(viewModel.ActiveView);
						}
					});
				}
			}
		};

		process.Start();
		process.BeginOutputReadLine();
		process.WaitForExit();
	}

	private void BrightnessChanged()
	{
		this.Opacity = Statics.CurrentBrightness;
	}
}