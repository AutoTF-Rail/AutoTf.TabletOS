using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DynamicData;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainSelectionScreen : UserControl
{
	private ObservableCollection<TrainAd> _nearbyTrains = new ObservableCollection<TrainAd>();
	
	public TrainSelectionScreen()
	{
		InitializeComponent();
		NearbyTrains.ItemsSource = _nearbyTrains;
		
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
		RunBridgeScan();
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

	private async void RunBridgeScan()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo()
		{
			FileName = "btmgmt",
			Arguments = "find",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		Process process = new Process()
		{
			StartInfo = processStartInfo
		};

		process.Start();
		Thread.Sleep(1500);

		StreamReader outputReader = process.StandardOutput;

		string? line;
		while ((line = await outputReader.ReadLineAsync()) != null)
		{
			if(line.Contains("name"))
				AddBridge(line.Replace("name ", ""));
		}

		Dispatcher.UIThread.Invoke(() =>
		{
			if (NearbyLoadingArea.IsVisible)
				NearbyTrains.IsVisible = false;
		});
	}

	private void AddBridge(string name)
	{
		Console.WriteLine("Adding: " + name);
		Dispatcher.UIThread.Invoke(() =>
		{
			_nearbyTrains.Add(new TrainAd()
			{
				TrainName = name
			});
			
			if (NearbyLoadingArea.IsVisible)
				NearbyTrains.IsVisible = false;
		});
	}
}