using System;
using System.IO;
using System.Threading.Tasks;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class InfoScreen : UserControl
{
	private readonly object _previousControl;

	public InfoScreen(object previousControl)
	{
		_previousControl = previousControl;

		InitializeComponent();

		Initialize();
	}

	private void Initialize()
	{
		VersionBox.Text = "Version: " + Program.GetGitVersion();
		UpdateButton.IsVisible = NetworkManager.IsInternetAvailable();
	}

	private void Button_OnClick(object? sender, RoutedEventArgs e)
	{
		if (DataContext is MainWindowViewModel viewModel)
		{
			viewModel.ActiveView = _previousControl;
		}
	}

	private async void Update_Click(object? sender, RoutedEventArgs e)
	{
		if (!NetworkManager.IsInternetAvailable())
			return;

		Dispatcher.UIThread.InvokeAsync(() => UpdateText.IsVisible = true);
		await Task.Delay(50);
		
		string prevDir = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			UpdateText.IsVisible = true;
			InfoStatus.IsVisible = true;
			InfoOutput.IsVisible = true;
		});
		await Task.Delay(50);

		string evalOutput = Statics.ExecuteCommand("eval $(\"ssh-agent\")");
		Console.WriteLine(evalOutput);
		
		await Dispatcher.UIThread.InvokeAsync(() =>InfoOutput.Text = evalOutput);
		await Task.Delay(50);

		string add = Statics.ExecuteCommand("ssh-add /home/display/githubKey");
		Console.WriteLine(add);
		await Dispatcher.UIThread.InvokeAsync(() => InfoOutput.Text = add);
		await Task.Delay(50);
		
		string aasddd = Statics.ExecuteCommand("git reset --hard");
		Console.WriteLine(aasddd);
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = aasddd;
			InfoStatus.Text = "Pulling";
		});
		await Task.Delay(50);
		
		string pull = Statics.ExecuteCommand("git pull");
		Console.WriteLine(pull);
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = pull;
			InfoStatus.Text = "Building";
		});
		await Task.Delay(50);
		
		string build = Statics.ExecuteCommand("dotnet build -c RELEASE -m");
		Console.WriteLine(build);
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = build;
			InfoStatus.Text = "Perms";
		});
		await Task.Delay(50);
		
		string perms = Statics.ExecuteCommand("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh");
		Console.WriteLine(perms);
		
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			InfoOutput.Text = perms;
		
			InfoStatus.Text = "Reboot";
			InfoOutput.Text = Statics.ExecuteCommand("reboot now");
		});
		await Task.Delay(50);
		
		Directory.SetCurrentDirectory(prevDir);
	}
}