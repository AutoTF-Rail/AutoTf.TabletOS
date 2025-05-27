using AutoTf.TabletOS.Models.Enums;
using ReactiveUI;

namespace AutoTf.TabletOS.Models;

public class TrainCameraInformation : ReactiveObject
{
    private Side _currentCamera = Side.Front;
    private Side _currentDirection = Side.Front;
    
    public Side CurrentCamera
    {
        get => _currentCamera;
        set => this.RaiseAndSetIfChanged(ref _currentCamera, value);
    }
    
    // TODO: Sync with train on startup
    public Side CurrentDirection
    {
        get => _currentDirection;
        set => this.RaiseAndSetIfChanged(ref _currentDirection, value);
    }
}