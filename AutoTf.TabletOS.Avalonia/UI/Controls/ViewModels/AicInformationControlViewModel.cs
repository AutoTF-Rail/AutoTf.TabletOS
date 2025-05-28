using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;

public class AicInformationControlViewModel : ViewModelBase
{
    private readonly IViewRouter _viewRouter;
    private readonly AicInformation _aicInformation;
    private readonly Logger _logger;

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

    public AicInformationControlViewModel(IViewRouter viewRouter, AicInformation aicInformation, Logger logger)
    {
        _viewRouter = viewRouter;
        _aicInformation = aicInformation;
        _logger = logger;
        AicCommand = new AsyncRelayCommand(OpenAicDialog);
    }

    protected override async Task Initialize()
    {
        string[] splashes = await File.ReadAllLinesAsync("CopiedAssets/AiSplash");
        
        await _aicInformation.UpdateState();
        
        AicStatus = _aicInformation.State;
        AicStatusBrush = _aicInformation.Color;
        SplashText = splashes[new Random().Next(splashes.Length)];
    }

    private async Task OpenAicDialog()
    {
        await _viewRouter.ShowDialog<AicControlView, AicControlViewModel>();
    }
}