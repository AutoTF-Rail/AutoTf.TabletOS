using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using Yubico.YubiKey;
using Yubico.YubiKey.Oath;

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
						LoadingName.Text = "Getting key..";
						LoadingArea.IsVisible = true;
						
						IYubiKeyDevice? device = YubiKeyDevice.FindAll().FirstOrDefault();
						if (device == null)
							return;

						using (OathSession session = new OathSession(device))
						{
							foreach (Credential credential in session.GetCredentials())
							{
								if (credential.Issuer != "AutoTF")
									return;
								Statics.YubiCode = session.CalculateCredential(credential).Value!;
								Statics.YubiSerial = device.SerialNumber!.Value;
								Statics.YubiTime = DateTime.UtcNow;
							}
						}
						// TODO: Error handling if no cred was found.
						if (DataContext is MainWindowViewModel viewModel)
						{
							viewModel.ActiveView = new TrainSelectionScreen();
						}
						LoadingArea.IsVisible = false;
					});
					// TODO: Requires ppa:yubico/stable - yubikey-manager
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