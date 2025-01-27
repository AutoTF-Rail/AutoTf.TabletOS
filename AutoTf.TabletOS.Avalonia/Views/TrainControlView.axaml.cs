using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
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
		Task.Run(InitializeStream);
	}
	
	private async Task InitializeStream()
	{
		try
		{
			string url = "ws://192.168.1.1/stream";

			using ClientWebSocket ws = new ClientWebSocket();
			await ws.ConnectAsync(new Uri(url), CancellationToken.None);

			byte[] buffer = new byte[4096];
			MemoryStream ms = new MemoryStream();

			while (ws.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				if (result.MessageType == WebSocketMessageType.Close)
				{
					await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
					break;
				}

				ms.Write(buffer, 0, result.Count);

				if (result.EndOfMessage)
				{
					ms.Seek(0, SeekOrigin.Begin);

					Bitmap bitmap = new Bitmap(ms);

					Dispatcher.UIThread.Invoke(() =>
					{
						PreviewImage.Source = bitmap;
					});

					ms.SetLength(0);
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error while getting preview:");
			Console.WriteLine(ex.Message);
		}
	}
	
	private string ExecuteCommand(string command)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			}
		};

		process.Start();
		string result = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		
		process.WaitForExit();
		
		if (result == "")
			return error;
		
		return result;
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