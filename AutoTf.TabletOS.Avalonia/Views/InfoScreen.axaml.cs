using System;
using System.IO;
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

	private void Update_Click(object? sender, RoutedEventArgs e)
	{
		if (!NetworkManager.IsInternetAvailable())
			return;

		Dispatcher.UIThread.Invoke(() => UpdateText.IsVisible = true);

		string prevDir = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory("/home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia");
		
		Dispatcher.UIThread.Invoke(() =>
		{
			UpdateText.IsVisible = true;
			InfoStatus.IsVisible = true;
			InfoOutput.IsVisible = true;

			string evalOutput = Statics.ExecuteCommand("eval $(\"ssh-agent\")");
			Console.WriteLine(evalOutput);
			InfoOutput.Text = evalOutput;

			string add = Statics.ExecuteCommand("ssh-add /home/display/githubKey");
			Console.WriteLine(add);
			InfoOutput.Text = add;
			
			string aasddd = Statics.ExecuteCommand("git reset --hard");
			Console.WriteLine(aasddd);
			InfoOutput.Text = aasddd;
			
			InfoStatus.Text = "Pulling";
			string pull = Statics.ExecuteCommand("git pull");
			Console.WriteLine(pull);
			InfoOutput.Text = pull;
			
			InfoStatus.Text = "Building";
			string build = Statics.ExecuteCommand("dotnet build -c RELEASE -m");
			Console.WriteLine(build);
			InfoOutput.Text = build;
			
			InfoStatus.Text = "Perms";
			string perms = Statics.ExecuteCommand("chmod +x /home/display/AutoTf.TabletOS/AutoTf.TabletOS/scripts/startup.sh");
			Console.WriteLine(perms);
			InfoOutput.Text = perms;
			
			InfoStatus.Text = "Reboot";
			InfoOutput.Text = Statics.ExecuteCommand("reboot now");
		});
		
		Directory.SetCurrentDirectory(prevDir);
	}
}