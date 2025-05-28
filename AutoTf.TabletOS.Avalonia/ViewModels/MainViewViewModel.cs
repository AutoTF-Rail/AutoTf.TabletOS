using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Yubico.YubiKey;
using Yubico.YubiKey.Oath;

namespace AutoTf.TabletOS.Avalonia.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    private bool _skipButtonVisible = true;
    
    private readonly IViewRouter _viewRouter;
    private readonly INotificationService _notificationService;
    private readonly YubiKeySession _yubiKey;

    private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
    private YubiKeyDeviceListener _listener;
    private bool _isHandlingKey;

    public bool SkipButtonVisible
    {
        get => _skipButtonVisible;
        private set => this.RaiseAndSetIfChanged(ref _skipButtonVisible, value);
    }

    public ICommand SkipButtonCommand { get; }

    public MainViewViewModel(IViewRouter viewRouter, INotificationService notificationService, YubiKeySession yubiKey)
    {
        _viewRouter = viewRouter;
        _notificationService = notificationService;
        _yubiKey = yubiKey;
#if RELEASE
        SkipButtonVisible = false;
#else
        SkipButtonVisible = true;
#endif
        SkipButtonCommand = new RelayCommand(SkipKeyLogin);
		
        _listener = YubiKeyDeviceListener.Instance;
        _listener.Arrived += async (sender, args) => await KeyPluggedIn(sender, args);
        _listener.Removed += KeyRemoved;
    }
    
    protected override async Task Initialize()
    {
        if (_isHandlingKey)
            return;
		
        _isHandlingKey = true;
        _viewRouter.InvokeLoadingArea(true, "Checking for key...");
        await Task.Delay(25);
        IYubiKeyDevice? key = YubiKeyDevice.FindAll().FirstOrDefault();
        if (key == null)
        {
            _viewRouter.InvokeLoadingArea(false);
            return;
        }
        await Task.Run(() => GetKey(key), _cancelTokenSource.Token);
    }

    private void KeyRemoved(object? sender, YubiKeyDeviceEventArgs e)
    {
        _isHandlingKey = false;
        _cancelTokenSource.Cancel();
        _viewRouter.InvokeLoadingArea(false);
    }

    private async Task KeyPluggedIn(object? _, YubiKeyDeviceEventArgs e)
    {
        if (_isHandlingKey)
            return;
        _isHandlingKey = true;
        _viewRouter.InvokeLoadingArea(true, "Loading key...");
        await Task.Delay(25);
        await Task.Run(() => GetKey(e.Device), _cancelTokenSource.Token);
    }

    private void GetKey(IYubiKeyDevice device)
    {
        using OathSession session = new OathSession(device);

        Credential? atfCred = session.GetCredentials().FirstOrDefault(c => c.Issuer == "AutoTF");

        if (atfCred is not null)
        {
            _yubiKey.Code = session.CalculateCredential(atfCred).Value!;
            _yubiKey.SerialNumber = device.SerialNumber!.Value;
            _yubiKey.Time = DateTime.UtcNow;
            ChangeScreen();
        }
        else
            _notificationService.Warn("Could not find AutoTF Credential on key.");

        _viewRouter.InvokeLoadingArea(false);
    }

    private void SkipKeyLogin()
    {
        _yubiKey.Empty();
        ChangeScreen();
    }

    private void ChangeScreen()
    {
        _viewRouter.NavigateTo<Views.TrainSelectionView>();
        _listener.Dispose();
    }
}