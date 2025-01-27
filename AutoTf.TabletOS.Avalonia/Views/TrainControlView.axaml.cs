using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainControlView : UserControl
{
	private readonly IDataManager _dataManager = Statics.DataManager;
	private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;
	private readonly System.Timers.Timer _syncTimer = new System.Timers.Timer(600);
	
	public TrainControlView()
	{
		InitializeComponent();

		Task.Run(Initialize);
	}
	
	private void StartInternetListener()
	{
		_syncTimer.Elapsed += SyncSyncTimerElapsed;
		_syncTimer.AutoReset = true;
		_syncTimer.Start();
	}

	private async void SyncSyncTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		try
		{
			string url = "http://192.168.1.1/information/latestFramePreview";

			using HttpClient loginClient = new HttpClient();
			loginClient.Timeout = TimeSpan.FromMilliseconds(600);
			
			HttpResponseMessage response = await loginClient.GetAsync(url);
			
			if (response.IsSuccessStatusCode)
			{
				byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

				using (MemoryStream ms = new MemoryStream(imageBytes))
				{
					Bitmap bitmap = new Bitmap(ms);
					Dispatcher.UIThread.Invoke(() => PreviewImage.Source = bitmap);
				}
			}
			else
			{
				Console.WriteLine("Failed to load image");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error while getting preview:");
			Console.WriteLine(ex.Message);
		}
	}

	private async void Initialize()
	{
		StartInternetListener();
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			LoadingName.Text = "Loading data...";
			LoadingArea.IsVisible = true;
		});

		await LoadLastConnected();
		await LoadTrainData();

		await Dispatcher.UIThread.InvokeAsync(() => LoadingArea.IsVisible = false);
	}

	private async Task LoadTrainData()
	{
		string? evuName = await _trainInfo.GetEvuName();
		string? trainId = await _trainInfo.GetTrainId();
		string? trainName = await _trainInfo.GetTrainName();
		string? lastTrainSync = await _trainInfo.GetLastSync();
		
		if (evuName == null || trainId == null || trainName == null || lastTrainSync == null)
		{
			// TODO: Disconnect
			// TODO: Log
			return;
		}

		int dayDiff = (DateTime.Parse(lastTrainSync) - DateTime.Now).Days * -1;

		IImmutableSolidColorBrush brush = ConvertDayIntoBrush(dayDiff);
		await Dispatcher.UIThread.InvokeAsync(() => NextTrainConnectionDay.Foreground = brush);

		if (dayDiff >= 30)
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextTrainConnectionDay.Text = $"Long due.");
		else
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextTrainConnectionDay.Text = (30 - dayDiff) + " Days");
		
		await Dispatcher.UIThread.InvokeAsync(() => EvuNameBox.Text = evuName);
		await Dispatcher.UIThread.InvokeAsync(() => TrainIdBox.Text = trainId);
		await Dispatcher.UIThread.InvokeAsync(() => TrainNameBox.Text = trainName);
	}

	private async Task LoadLastConnected()
	{
		int dayDiff = (_dataManager.GetLastSynced() - DateTime.Now).Days * -1;
		
		IImmutableSolidColorBrush brush = ConvertDayIntoBrush(dayDiff);
		await Dispatcher.UIThread.InvokeAsync(() => NextConnectionDay.Foreground = brush);

		if (dayDiff >= 30)
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextConnectionDay.Text = $"Please connect to the internet as soon as possible.");
		else
			await Dispatcher.UIThread.InvokeAsync(() =>
				NextConnectionDay.Text = $"Please connect to the internet in {30 - dayDiff} days.");
	}

	private IImmutableSolidColorBrush ConvertDayIntoBrush(int dayDif)
	{
		return dayDif switch
		{
			< 10 => Brushes.Green,
			< 20 => Brushes.Yellow,
			_ => Brushes.Red
		};
	}

	private void ChangeTrain_Click(object? sender, RoutedEventArgs e)
	{
		// Tell train that you disconnected (emergency break if connection is lost, or user proceeds)
		// Stop streams
		// Disconnect from wifi
		// Change screen
		
		
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = new TrainSelectionScreen();
		}
	}
}