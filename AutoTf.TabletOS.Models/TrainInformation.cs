using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public class TrainInformation
{
    private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;
    private readonly Logger _logger = Statics.Logger; 

    public TrainInformation()
    {
        Initialize();
    }

    private async void Initialize()
    {
        try
        {
            EvuName = (await _trainInfo.GetEvuName()).GetValue("Unknown");
            TrainId = (await _trainInfo.GetTrainId()).GetValue("Unknown");
            TrainName = (await _trainInfo.GetTrainName()).GetValue("Unknown");
            TrainVersion = (await _trainInfo.GetVersion()).GetValue("Unknown");

            InitializedSuccessfully = true;
        }
        catch (Exception e)
        {
            _logger.Log("An error occured while getting some train information:");
            _logger.Log(e.ToString());
        }
    }

    // We still set the default value here, just in case the initialize method fails before all values are loaded.
    public string EvuName { get; private set; } = "Unknown";
    public string TrainId { get; private set; } = "Unknown";
    public string TrainName { get; private set; } = "Unknown";
    public string TrainVersion { get; private set; } = "Unknown";
    
    public bool InitializedSuccessfully { get; private set; }
}