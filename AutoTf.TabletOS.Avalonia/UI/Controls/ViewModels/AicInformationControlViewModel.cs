using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;

public class AicInformationControlViewModel : ReactiveObject
{
    private readonly IViewRouter _viewRouter;
    private readonly AicInformation _aicInformation;

    private string _aicStatus = "Offline";
    private string _splashText = "Loading";
    private IImmutableSolidColorBrush _aicStatusBrush = Brushes.Red;
    
    public string AicStatus
    {
        get => _aicStatus;
        private set => this.RaiseAndSetIfChanged(ref _aicStatus, value);
    }
    
    public string SplashText
    {
        get => _splashText;
        private set => this.RaiseAndSetIfChanged(ref _splashText, value);
    }
    
    public IImmutableSolidColorBrush AicStatusBrush
    {
        get => _aicStatusBrush;
        private set => this.RaiseAndSetIfChanged(ref _aicStatusBrush, value);
    }
    
    public IAsyncRelayCommand AicCommand { get; }

    public AicInformationControlViewModel(IViewRouter viewRouter, AicInformation aicInformation)
    {
        _viewRouter = viewRouter;
        _aicInformation = aicInformation;
        AicCommand = new AsyncRelayCommand(OpenAicDialog);

        Initialize();
    }
    
    private async void Initialize()
    {
        string[] splashes = await File.ReadAllLinesAsync("CopiedAssets/AiSplash");
        
        await _aicInformation.UpdateState();

        AicStatus = _aicInformation.State;
        AicStatusBrush = _aicStatusBrush;
        SplashText = splashes[new Random().Next(splashes.Length)];
    }

    private async Task OpenAicDialog()
    {
        await _viewRouter.ShowDialog<AicControlView, AicControlViewModel>();
    }
}