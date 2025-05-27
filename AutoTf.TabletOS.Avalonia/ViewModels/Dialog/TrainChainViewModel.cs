using System.Collections.ObjectModel;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Models;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

public class TrainChainViewModel : DialogViewModelBase
{
    private ObservableCollection<TrainAd> _trainsNear = new ObservableCollection<TrainAd>();
    
    public ObservableCollection<TrainAd> TrainsNear
    {
        get => _trainsNear;
        set => this.RaiseAndSetIfChanged(ref _trainsNear, value);
    }

    public TrainAd ExampleTrainAd =>
        new TrainAd()
        {
            TrainName = "Test",
            TrainNum = "000-000"
        };
    
    public IRelayCommand BackCommand { get; }
    public IRelayCommand<TrainAd> RemoveCommand { get; }
    
    public TrainChainViewModel()
    {
        BackCommand = new RelayCommand(Back);
        RemoveCommand = new RelayCommand<TrainAd>(RemoveTrain);
        
        // TODO: Remove train removal button where the tablet is currently connected.
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
        TrainsNear.Add(new TrainAd()
        {
            TrainName = "Guh",
            TrainNum = "233-123"
        });
    }

    private void RemoveTrain(TrainAd? obj)
    {
        
        // Popup popup = new Popup("Are you sure you want to remove this train?");
		      //
        // if (await popup.Show(RootGrid) != DialogResult.Yes)
        //     return;
    }

    private void Back()
    {
        Close(0);
    }
}