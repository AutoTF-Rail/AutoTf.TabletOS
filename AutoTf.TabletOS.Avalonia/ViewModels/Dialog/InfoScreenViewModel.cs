using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoTf.CentralBridge.Shared.Models;
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

    private string _gitVersion = "Unknown";
    private string _infoText = "";
    private string _switchBranchText = "Feature unavailable";

    private bool _canSwitch;
    private string _nextBranch = "develop";

    
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
    
    public string SwitchBranchText
    {
	    get => _switchBranchText;
	    private set => this.RaiseAndSetIfChanged(ref _switchBranchText, value);
    }
    
    public IRelayCommand BackCommand { get; }
    public IAsyncRelayCommand UpdateCommand { get; }
    public IAsyncRelayCommand LogsCommand { get; }
    public IAsyncRelayCommand SwitchBranchCommand { get; }
    
    public InfoScreenViewModel(IViewRouter viewRouter, INetworkService networkService)
    {
        _viewRouter = viewRouter;
        _networkService = networkService;

        BackCommand = new RelayCommand(() => Close(0));
        UpdateCommand = new AsyncRelayCommand(() => Update(), NetworkService.IsInternetAvailable);
        SwitchBranchCommand = new AsyncRelayCommand(SwitchBranch, () => _canSwitch);
        LogsCommand = new AsyncRelayCommand(OpenLogs);
    }

    private async Task SwitchBranch()
    {
	    if (!_canSwitch)
		    return;

	    await Update(_nextBranch);
	    // We still need to update the text afterwards, because the Update could fail.
	    await Initialize();
    }

    protected override Task Initialize()
    {
	    GitVersion = $"Version: {Program.GetGitVersion()}";
	    List<string> branches = Program.GetBranches();
	    
	    if (branches.Any())
		    _canSwitch = true;

	    string? currentBranch = branches.FirstOrDefault(x => x.StartsWith("* "));

	    if (currentBranch == null)
	    {
		    _nextBranch = "main";
		    return Task.CompletedTask;
	    }
	    currentBranch = currentBranch.Replace("*", "").Trim();

	    string nextBranchToSearch = "develop";
	    
	    if (currentBranch == "develop")
		    nextBranchToSearch = "main";

	    string? nextBranch = branches.FirstOrDefault(x => x.ToLower().Contains(nextBranchToSearch));

	    if (nextBranch == null)
	    {
		    _canSwitch = false;
		    return Task.CompletedTask;
	    }

	    _nextBranch = nextBranch;
	    SwitchBranchText = $"Switch To {_nextBranch}";
	    
	    return Task.CompletedTask;
    }

    private async Task Update(string branch = "main")
    {
	    if (!NetworkService.IsInternetAvailable())
		    return;
	    
	    string prevDir = Directory.GetCurrentDirectory();
	    await _viewRouter.InvokeLoadingArea(true, "Updating...");
	    
	    InfoText = "";
	    
	    Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
	    
	    try
	    {
			CommandExecuter.ExecuteCommand("git reset --hard");
			
			CommandExecuter.ExecuteCommand($"git checkout {branch}");
			
			await _viewRouter.InvokeLoadingArea(true, "Downloading updates...");
			
			string pull = CommandExecuter.ExecuteCommand("git pull");
			
			if (pull.Contains("Already"))
			{
				InfoText = "Already Up to Date.";
				return;
			}
			CommandExecuter.ExecuteCommand("git submodule update --init --recursive");
			
			await _viewRouter.InvokeLoadingArea(true, "Building update...");
			
			string buildOutput = CommandExecuter.ExecuteCommand("dotnet build -c RELEASE -m");
			if (!buildOutput.Contains("0 Error(s)"))
			{
				InfoText = "Failed to build the update.";
				return;
			}
			
			CommandExecuter.ExecuteCommand("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh");
			_networkService.ShutdownConnection();
			
			await _viewRouter.InvokeLoadingArea(true, "Rebooting");
			InfoText = "Restarting.";
			
			CommandExecuter.ExecuteSilent("reboot now", true);
	    }
	    finally
	    {
		    
		    CommandExecuter.ExecuteSilent("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh", true);
		    Directory.SetCurrentDirectory(prevDir);
		    await _viewRouter.InvokeLoadingArea(false);
	    }
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