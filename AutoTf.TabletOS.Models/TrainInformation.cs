using AutoTf.CentralBridge.Shared.Models;
using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Models;

public class TrainInformation
{
    private readonly ITrainInformationService _trainInfo = Statics.TrainInformationService;
    private readonly Logger _logger = Statics.Logger; 
    
    public async Task UpdateData()
    {
        try
        {
            Task<Result<string>> evuNameTask = _trainInfo.GetEvuName();
            Task<Result<string>> trainIdTask = _trainInfo.GetTrainId();
            Task<Result<string>> trainNameTask = _trainInfo.GetTrainName();
            Task<Result<string>> versionTask = _trainInfo.GetVersion();
            
            await Task.WhenAll(evuNameTask, trainIdTask, trainNameTask, versionTask);

            if (!evuNameTask.Result.IsSuccess)
                _logger.Log($"Could not get evu name: [{evuNameTask.Result.ResultCode}] {evuNameTask.Result.Value}.");
            
            if (!trainIdTask.Result.IsSuccess)
                _logger.Log($"Could not get train ID: [{trainIdTask.Result.ResultCode}] {trainIdTask.Result.Value}.");
            
            if (!trainNameTask.Result.IsSuccess)
                _logger.Log($"Could not get train name: [{trainNameTask.Result.ResultCode}] {trainNameTask.Result.Value}.");
            
            if (!versionTask.Result.IsSuccess)
                _logger.Log($"Could not get train version: [{versionTask.Result.ResultCode}] {versionTask.Result.Value}.");
            
            EvuName = evuNameTask.Result.GetValue("Unknown");
            TrainId = trainIdTask.Result.GetValue("Unknown");
            TrainName = trainNameTask.Result.GetValue("Unknown");
            TrainVersion = versionTask.Result.GetValue("Unknown");

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