using System;
using System.Threading.Tasks;
using Autofac;
using AutoTf.Logging;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Base;

public class ViewModelBase : ReactiveObject
{
    private readonly Logger _logger;

    protected ViewModelBase()
    {
        _logger = App.Container!.Resolve<Logger>();
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        try
        {
            await Initialize();
        }
        catch (Exception ex)
        {
            _logger.Log("Failed to initialize Viewmodel: " );
            _logger.Log(ex.ToString());
        }
    }

    protected virtual Task Initialize()
    {
        return Task.CompletedTask;
    }
}