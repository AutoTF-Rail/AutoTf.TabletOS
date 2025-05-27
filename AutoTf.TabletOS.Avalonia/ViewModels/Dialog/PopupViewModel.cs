using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

public class PopupViewModel : DialogViewModelBase
{
    private string _message = "Offline";
    private bool _showCancel;
    
    public string Message
    {
        get => _message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }
    
    public bool ShowCancel
    {
        get => _showCancel;
        private set => this.RaiseAndSetIfChanged(ref _showCancel, value);
    }
    
    public IRelayCommand YesCommand { get; }
    public IRelayCommand NoCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public PopupViewModel(string message, bool showCancel = false)
    {
        YesCommand = new RelayCommand(() => Close(0));
        NoCommand = new RelayCommand(() => Close(1));
        CancelCommand = new RelayCommand(() => Close(2));

        Message = message;
        ShowCancel = showCancel;
    }

    public static Popup Create(string message, bool showCancel = false)
    {
        PopupViewModel viewModel = new PopupViewModel(message, showCancel);
        return new Popup(viewModel);
    }
}