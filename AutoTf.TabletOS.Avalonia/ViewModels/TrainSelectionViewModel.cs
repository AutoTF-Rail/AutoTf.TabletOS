using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.Views;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using AutoTf.TabletOS.Models.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels;

public class TrainSelectionViewModel : ViewModelBase
{
    private readonly Logger _logger;
    private readonly INetworkService _networkService;
    private readonly IViewRouter _viewRouter;
    private readonly INotificationService _notificationService;
    private readonly YubiKeySession _yubiKey;

    private ObservableCollection<TrainAd> _nearbyTrains = new ObservableCollection<TrainAd>();
    private bool _nearbyLoadingVisible = true;
    
    public ObservableCollection<TrainAd> NearbyTrains
    {
        get => _nearbyTrains;
        private set => this.RaiseAndSetIfChanged(ref _nearbyTrains, value);
    }
    
    public bool NearbyLoadingVisible
    {
        get => _nearbyLoadingVisible;
        private set => this.RaiseAndSetIfChanged(ref _nearbyLoadingVisible, value);
    }
    
    public IRelayCommand<TrainAd> TrainNearbyCommand { get; }
    public IRelayCommand RescanCommand { get; }

    public TrainSelectionViewModel(Logger logger, INetworkService networkService, IViewRouter viewRouter, INotificationService notificationService, YubiKeySession yubiKey)
    {
        _logger = logger;
        _networkService = networkService;
        _viewRouter = viewRouter;
        _notificationService = notificationService;
        _yubiKey = yubiKey;
        TrainNearbyCommand = new AsyncRelayCommand<TrainAd>(HandleNearbyTrain);
        RescanCommand = new AsyncRelayCommand(RunTrainScan);
        
        _viewRouter.InvokeLoadingArea(true, "Loading data...");
#if DEBUG
        _nearbyTrains.Add(new TrainAd()
        {
            TrainName = "ExampleTrain",
            TrainNum = "783-938"
        });
        NearbyLoadingVisible = false;
        _viewRouter.InvokeLoadingArea(false);
#endif
    }

    protected override async Task Initialize()
    {
        await _viewRouter.InvokeLoadingArea(false);
        LoadInternetTrains();
        await RunTrainScan();
    }
    
    private async Task RunTrainScan()
    {
        try
        {
#if DEBUG
            return;
            _logger.Log("Not running bluetooth scan due to not being in RELEASE");
#endif
            NearbyLoadingVisible = true;
            await Task.Delay(25);
            _logger.Log("Scanning for trains.");
            NearbyTrains.Clear();
            
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "timeout",
                Arguments = "6s btmgmt find",
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process()
            {
                StartInfo = processStartInfo
            };

            process.Start();
            process.WaitForExit(6000);

            StreamReader outputReader = process.StandardOutput;

            while (await outputReader.ReadLineAsync() is { } line)
            {
                if (line.Contains("name") && line.Contains("CentralBridge-"))
                {
                    if (_nearbyTrains.Any(x => x.TrainName == line.Replace("name ", "")))
                        continue;
                    AddBridge(line.Replace("name ", ""));
                }
            }
            
            _logger.Log("Done scanning for nearby devices");
        }
        catch (Exception e)
        {
            _logger.Log("Train scan error:");
            _logger.Log(e.ToString());
            _notificationService.Error("An error occured while scanning for trains nearby.");
        }
        
        NearbyLoadingVisible = false;
    }

    private void AddBridge(string name)
    {
        _logger.Log("Adding train: " + name);
        _nearbyTrains.Add(new TrainAd()
        {
            TrainName = name
        });
    }

    private void LoadInternetTrains()
    {
        // if (NetworkService.IsInternetAvailable())
        // {
        //     Dispatcher.UIThread.Invoke(() => OtherTrainsLoadingText.Text = "No trains found.");
        // }
        // else
        // {
        //     Dispatcher.UIThread.Invoke(() => OtherTrainsLoadingText.Text = "No Internet Connection.");
        // }
    }

    private async Task HandleNearbyTrain(TrainAd? trainAd)
    {
        try
        {
            if (trainAd == null)
                return;
            
            await _viewRouter.InvokeLoadingArea(true, "Connecting to train...");
            // Idk why this extra call is needed JUST here
            await Task.Yield();
            
            bool isTestTrain = trainAd.TrainName == "ExampleTrain" && trainAd.TrainNum == "783-938";
            Statics.LoadedTestTrain = isTestTrain;

            if (isTestTrain)
            {
                _notificationService.Info("Loading test train.");
                await _viewRouter.InvokeLoadingArea(true, "Loading panel...");
                await _viewRouter.NavigateTo<TrainControlView>();
                return;
            }

            Statics.TrainConnectionId = Statics.GenerateRandomString();

            _logger.Log($"Trying to connect to train with SSID: {trainAd.TrainName}.");
            string? connOutput = _networkService.EstablishConnection(trainAd.TrainName, true);

            if(Statics.Connection == ConnectionType.None)
            {
                _logger.Log($"Connection: {Statics.Connection}.");
                _logger.Log(connOutput!);
                await _viewRouter.InvokeLoadingArea(false);
                
                _notificationService.Warn("Could not connect to train.");
                return;
            }

            await TryLogin();
        }
        catch (Exception exception)
        {
            _logger.Log("Could not connect to train:");
            _logger.Log(exception.ToString());
            _notificationService.Error("Could not connect to train. An internal error occured.");
        }
    }
    
    private async Task TryLogin()
    {
        await _viewRouter.InvokeLoadingArea(true, "Logging in...");
		
        try
        {
            string url = "http://192.168.1.1/information/login?macAddr=" + Statics.MacAddress + "&serialNumber=" + _yubiKey.SerialNumber + "&code=" + _yubiKey.Code + "&timestamp=" + _yubiKey.Time.ToString("yyyy-MM-ddTHH:mm:ss");

            using HttpClient loginClient = new HttpClient();
			
            HttpResponseMessage loginResponse = await loginClient.PostAsync(url, new StringContent(""));
			
            loginResponse.EnsureSuccessStatusCode();
			
            await _viewRouter.InvokeLoadingArea(true, "Loading panel...");
            await _viewRouter.NavigateTo<TrainControlView>();
        }
        catch (Exception ex)
        {
            _networkService.ShutdownConnection();
            _logger.Log("Could not login:");
            _logger.Log(ex.ToString());
            
            await _viewRouter.InvokeLoadingArea(false);
            _notificationService.Error("Could not login. An internal error occured.");
        }
    }
}