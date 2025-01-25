using System.Collections.Generic;
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
		Dispatcher.UIThread.Invoke(() =>
		{
			NearbyTrains.ItemsSource = new List<TrainAd>()
			{
				new TrainAd()
				{
					TrainName = "Meow",
					TrainNum = "123-123"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				},
				new TrainAd()
				{
					TrainName = "Wuff",
					TrainNum = "456-456"
				}
			};
		});
		Thread.Sleep(2500);
		Dispatcher.UIThread.Invoke(() =>
		{
			NearbyLoadingArea.IsVisible = false;
		});
	}

	private void LoadInternetTrains()
	{
		if (NetworkManager.IsInternetAvailable())
		{
			// Show text that trains aren't available due to no internet
		}
	}
}