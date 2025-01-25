using System;
using System.Diagnostics;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();

		Statics.BrightnessChanged += BrightnessChanged;
		LoadingArea.IsVisible = false;

		ListenForYubikey();
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
					LoadingName.Text = "Loading key...";
					LoadingArea.IsVisible = true;
					Console.WriteLine("YubiKey plugged in: " + e.Data);
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