using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainSelectionScreen : UserControl
{
	public TrainSelectionScreen()
	{
		InitializeComponent();
		
		Task.Run(Initialize);
	}

	private void Initialize()
	{
		Dispatcher.UIThread.Invoke(() =>
		{
			LoadingArea.IsVisible = false;
		});
		
		LoadInternetTrains();
		LoadNearbyTrains();
	}

	private void LoadNearbyTrains()
	{
		string[]? bridges = RunBridgeScan();
		
		Dispatcher.UIThread.Invoke(() =>
		{
			NearbyLoadingArea.IsVisible = false;
			if (bridges != null)
			{
				NearbyTrains.ItemsSource = bridges.Select(x => new TrainAd()
				{
					TrainName = x.Replace("CentralBridge-", "") 
				}).ToList();
			}
		});
	}

	private void LoadInternetTrains()
	{
		if (NetworkManager.IsInternetAvailable())
		{
			// Show text that trains aren't available due to no internet
		}
	}

	private void NearbyTrains_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		NearbyTrains.SelectedItem = null;
	}

	private void OtherTrains_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		OtherTrains.SelectedItem = null;
	}
	
	public string[]? RunBridgeScan()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo()
		{
			FileName = "hcitool",
			Arguments = "lescan",
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		Process process = new Process()
		{
			StartInfo = processStartInfo
		};

		process.Start();
		Thread.Sleep(1500);
		process.Kill();

		string output = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		return FilterForBridge(output);
	}
	
	private string[]? FilterForBridge(string output)
	{
		Regex regex = new Regex(@"([0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2})\s*(CentralBridge-[^\s]+)");

		MatchCollection matches = regex.Matches(output);
		return matches.Select(x => x.Groups[2].Value).ToArray();
	}
}