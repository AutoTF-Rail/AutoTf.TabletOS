using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainSelectionScreen : UserControl
{
	public TrainSelectionScreen()
	{
		InitializeComponent();
		Task.Run(Initialize);
	}

	private void Initialize()
	{
		LoadInternetTrains();

		LoadingArea.IsVisible = false;
	}

	private void LoadInternetTrains()
	{
		if (NetworkManager.IsInternetAvailable())
		{
			// Show text that trains aren't available due to no internet
		}
	}
}