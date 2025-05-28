using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

public class InfoScreenViewModel : DialogViewModelBase
{
    private const string LogDir = "/var/log/AutoTF/AutoTf.TabletOS.Avalonia/";
    
    private readonly IViewRouter _viewRouter;
    private readonly INetworkService _networkService;
    private readonly Logger _logger;

    private string _gitVersion = "Unknown";
    private string _infoText = "";
    
    public string GitVersion
    {
	    get => _gitVersion;
	    private set => this.RaiseAndSetIfChanged(ref _gitVersion, value);
    }
    
    public string InfoText
    {
	    get => _infoText;
	    private set => this.RaiseAndSetIfChanged(ref _infoText, value);
    }

    public IRelayCommand BackCommand { get; }
    public IAsyncRelayCommand UpdateCommand { get; }
    public IRelayCommand RestartCommand { get; }
    public IAsyncRelayCommand LogsCommand { get; }
    

    public InfoScreenViewModel(IViewRouter viewRouter, INetworkService networkService, Logger logger)
    {
        _viewRouter = viewRouter;
        _networkService = networkService;
        _logger = logger;
        
        BackCommand = new RelayCommand(() => Close(0));
        UpdateCommand = new AsyncRelayCommand(Update, NetworkService.IsInternetAvailable);
        RestartCommand = new RelayCommand(Restart);
        LogsCommand = new AsyncRelayCommand(OpenLogs);
    }

    protected override Task Initialize()
    {
	    GitVersion = $"Version: {Program.GetGitVersion()}";
	    return Task.CompletedTask;
    }

    private void Restart()
    {
        try
        {
            _networkService.ShutdownConnection();
        }
        catch (Exception exception)
        {
            _logger.Log("Failed to shutdown connection:");
            _logger.Log(exception.ToString());
        }
        finally
        {
            CommandExecuter.ExecuteSilent("reboot now", true);
        }
    }

    private async Task Update()
    {
        if (!NetworkService.IsInternetAvailable())
			return;

        await _viewRouter.InvokeLoadingArea(true, "Updating...");
        InfoText = "";
        
		string prevDir = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
		
		CommandExecuter.ExecuteCommand("git reset --hard");
		
		await _viewRouter.InvokeLoadingArea(true, "Downloading updates...");
		
		string pull = CommandExecuter.ExecuteCommand("git pull");
		
		if (pull.Contains("Already"))
		{
			InfoText = "Already Up to Date.";
			
			CommandExecuter.ExecuteSilent("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh", true);
			await _viewRouter.InvokeLoadingArea(false);
			return;
		}
		CommandExecuter.ExecuteCommand("git submodule update --init --recursive");
		
		await _viewRouter.InvokeLoadingArea(true, "Building update...");
		
		string buildOutput = CommandExecuter.ExecuteCommand("dotnet build -c RELEASE -m");
		if (!buildOutput.Contains("0 Error(s)"))
		{
			InfoText = "Failed to build the update.";
			await _viewRouter.InvokeLoadingArea(false);
			return;
		}
		
		CommandExecuter.ExecuteCommand("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh");
		_networkService.ShutdownConnection();
		
		await _viewRouter.InvokeLoadingArea(true, "Rebooting");
		InfoText = "Restarting.";
		
		CommandExecuter.ExecuteSilent("reboot now", true);
		
		Directory.SetCurrentDirectory(prevDir);
		await _viewRouter.InvokeLoadingArea(false);
    }

    private async Task OpenLogs()
    {
        string[] files = [];
		
        if (Path.Exists(LogDir))
            files = Directory.GetFiles(LogDir).Order().ToArray();

        // Dirty ik
        RemoteLogsViewer view = RemoteLogsViewerViewModel.Create(files.Select(Path.GetFileNameWithoutExtension).ToArray()!, s => Task.FromResult(Result<string[]>.Ok(File.ReadAllLines(LogDir + s + ".txt"))));
        await _viewRouter.ShowDialog(view);
    }
}