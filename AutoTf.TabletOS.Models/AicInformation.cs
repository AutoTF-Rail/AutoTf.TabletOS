using AutoTf.Logging;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media;

namespace AutoTf.TabletOS.Models;

public class AicInformation
{
    private readonly IAicService _aicService;
    private readonly Logger _logger;

    public AicInformation(IAicService aicService, Logger logger)
    {
        _aicService = aicService;
        _logger = logger;
    }

    public string State { get; private set; } = "Offline";

    public IImmutableSolidColorBrush Color
    {
        get
        {
            if (State == "Offline")
                return Brushes.Red;

            if (State == "Disconnected")
                return Brushes.Yellow;

            return Brushes.Green;
        }
    }

    public async Task<bool> IsOnline() => await _aicService.IsOnline();

    public async Task UpdateState()
    {
        try
        {
            bool isOnline = (await _aicService.IsOnline()).IsSuccess;

            bool? isAvailable = null;
            
            if (isOnline)
                isAvailable = await _aicService.IsAvailable();
            
            if (!isOnline)
                State = "Offline";
            else if (isAvailable == null || (bool)!isAvailable)
                State = "Disconnected";
            else
                State = "Online";
        }
        catch (Exception e)
        {
            _logger.Log("An error occured while getting some aic information:");
            _logger.Log(e.ToString());
        }
    }
}