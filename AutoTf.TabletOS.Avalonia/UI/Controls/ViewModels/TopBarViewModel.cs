using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
#if RELEASE
using System.IO;
#endif
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using AutoTf.TabletOS.Avalonia.Views;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;

public class TopBarViewModel : ReactiveObject
{
    private DispatcherTimer _timer = null!;
    private int _brightness;
	
    private TaskCompletionSource<(bool success, string result)>? _keyboardTcs;
    private int _maxKeyboardTextLength = 0;
	
    private readonly INotificationService _notificationService;
    private readonly YubiKeySession _yubiKey;
    private readonly INetworkService _networkService;
    private readonly IViewRouter _viewRouter;

    private string _currentTime = "Unavailable";
    private string _notificationsNumber = "";
    private bool _quickMenuVisible;
    private bool _keyboardVisible;
    private ObservableCollection<Notification> _notifications = new ObservableCollection<Notification>();
    private string _keyboardValue;
    
    public string CurrentTime
    {
        get => _currentTime;
        private set => this.RaiseAndSetIfChanged(ref _currentTime, value);
    }
    
    public string NotificationsNumber
    {
        get => _notificationsNumber;
        private set => this.RaiseAndSetIfChanged(ref _notificationsNumber, value);
    }
    
    public bool QuickMenuVisible
    {
        get => _quickMenuVisible;
        private set => this.RaiseAndSetIfChanged(ref _quickMenuVisible, value);
    }
    
    public bool KeyboardVisible
    {
        get => _keyboardVisible;
        private set => this.RaiseAndSetIfChanged(ref _keyboardVisible, value);
    }
    
    public ObservableCollection<Notification> Notifications
    {
        get => _notifications;
        private set => this.RaiseAndSetIfChanged(ref _notifications, value);
    }
    
    public string KeyboardValue
    {
        get => _keyboardValue;
        private set => this.RaiseAndSetIfChanged(ref _keyboardValue, value);
    }
    
    public IRelayCommand ToggleQuickMenuCommand { get; }
    public IAsyncRelayCommand RestartCommand { get; }
    public IAsyncRelayCommand ShutdownCommand { get; }
    public IRelayCommand DarkerCommand { get; }
    public IRelayCommand BrighterCommand { get; }
    public IAsyncRelayCommand InfoCommand { get; }
    public IRelayCommand LogOutCommand { get; }
    
    public IRelayCommand CloseKeyboardCommand { get; }
    public IRelayCommand SaveKeyboardCommand { get; }
    public IRelayCommand<string> EnterKeyboardValueCommand { get; }
    public IRelayCommand DeleteKeyboardValueCommand { get; }
    
    public IRelayCommand<Notification> NotificationDiscardCommand { get; }

    public TopBarViewModel(INotificationService notificationService, YubiKeySession yubiKey, INetworkService networkService, IViewRouter viewRouter)
    {
        _notificationService = notificationService;
        _yubiKey = yubiKey;
        _networkService = networkService;
        _viewRouter = viewRouter;

        ToggleQuickMenuCommand = new RelayCommand(() => QuickMenuVisible = !QuickMenuVisible);
        RestartCommand = new AsyncRelayCommand(Restart);
        ShutdownCommand = new AsyncRelayCommand(Shutdown);
        DarkerCommand = new RelayCommand(() => ChangeBrightness(false));
        BrighterCommand = new RelayCommand(() => ChangeBrightness(true));
        InfoCommand = new AsyncRelayCommand(ShowInfo);
        LogOutCommand = new RelayCommand(LogOut);

        CloseKeyboardCommand = new RelayCommand(CloseKeyboard);
        SaveKeyboardCommand = new RelayCommand(SaveKeyboard);
        EnterKeyboardValueCommand = new RelayCommand<string>(EnterKeyboardValue);
        DeleteKeyboardValueCommand = new RelayCommand(DeleteKeyboardValue);

        NotificationDiscardCommand = new RelayCommand<Notification>(DiscardNotification);

        Initialize();
    }

    private void DiscardNotification(Notification? notification)
    {
        if (notification == null)
            return;
        
        _notificationService.Remove(notification);
    }

    private void DeleteKeyboardValue()
    {
        if (KeyboardValue.Length <= 1)
        {
            KeyboardValue = "0";
            return;
        }
				
        KeyboardValue = KeyboardValue.Substring(0, KeyboardValue.Length - 1);
    }

    private void EnterKeyboardValue(string? value)
    {
        if (value == null)
            return;

        if (value == ",")
        {
            if (string.IsNullOrEmpty(KeyboardValue) || KeyboardValue.Contains(','))
                return;
        }

        if (KeyboardValue.Length + 1 > _maxKeyboardTextLength)
            return;
		
        if (KeyboardValue == "0" && value != ",")
            KeyboardValue = value;
        else
            KeyboardValue += value;
    }

    private void SaveKeyboard()
    {
        KeyboardVisible = false;
		
        // Remove trailing comma
        if (KeyboardValue.Last() == ',')
            KeyboardValue = KeyboardValue.Substring(0, KeyboardValue.Length - 1);
		
        _keyboardTcs?.TrySetResult((true, KeyboardValue));
    }

    private void CloseKeyboard()
    {
        KeyboardVisible = false;
        _keyboardTcs?.TrySetResult((false, ""));
    }

    private void LogOut()
    {
        _yubiKey.Empty();
        _viewRouter.NavigateTo<MainView>();
    }

    private async Task ShowInfo()
    {
        await _viewRouter.ShowDialog<InfoScreen, InfoScreenViewModel>();
    }

    private void ChangeBrightness(bool add)
    {
        int newVal = _brightness;

        if (add)
            newVal += 25;
        else
            newVal -= 25;
        
        if (newVal is <= 50 or >= 255)
            return;
        
        _brightness = newVal;
        
#if RELEASE
		CommandExecuter.ExecuteSilent("echo " + _brightness +
		                              " | sudo tee /sys/class/backlight/10-0045/brightness", true);
#endif
    }

    private async Task Restart()
    {
        if (await _viewRouter.ShowDialog(PopupViewModel.Create("Are you sure you want to restart?")) != 0)
            return;
        
        Statics.Shutdown?.Invoke();
        _networkService.ShutdownConnection();
		
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "restart",
                Arguments = "now",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
		
        process.Start();
        Environment.Exit(0);
    }

    private async Task Shutdown()
    {
        if (await _viewRouter.ShowDialog(PopupViewModel.Create("Are you sure you want to shutdown?")) != 0)
            return;
		
        Statics.Shutdown?.Invoke();
        _networkService.ShutdownConnection();
		
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "now",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
		
        process.Start();
        Environment.Exit(0);
    }

    private void Initialize()
    {
        Statics.RegisterKeyboard(ShowKeyboard);
        
        Notifications = _notificationService.Notifications;
        _notificationService.Notifications.CollectionChanged += (_, _) =>
        {
            NotificationsNumber = new string('•', _notificationService.Notifications.Count);
        };
        
#if RELEASE
		_brightness = int.Parse(File.ReadAllText("/sys/class/backlight/10-0045/brightness"));
#endif
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1) 
        };
        _timer.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
        _timer.Start();
    }
    
    public async Task<(bool success, string result)> ShowKeyboard(string originalValue, int maxLength)
    {
        _keyboardTcs = new TaskCompletionSource<(bool, string)>();
        _maxKeyboardTextLength = maxLength;

        KeyboardValue = originalValue;
        KeyboardVisible = true;
        
        return await _keyboardTcs.Task;
    }
}