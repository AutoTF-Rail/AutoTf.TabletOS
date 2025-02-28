using System.Threading.Tasks;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Enums;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class TrainChainView : UserControl
{
	private TaskCompletionSource _taskCompletionSource = null!;
	private Grid _parent = null!;
	
	public TrainChainView()
	{
		// TODO: Remove train removal button where the tablet is currently connected.
		InitializeComponent();
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
		TrainsNearBox.Items.Add(new TrainAd()
		{
			TrainName = "Guh",
			TrainNum = "233-123"
		});
	}
	
	private void OtherTrains_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		TrainsNearBox.SelectedItem = null;
	}

	public Task Show(Grid parent)
	{
		_parent = parent;
		_taskCompletionSource = new TaskCompletionSource();
		parent.Children.Add(this);

		return _taskCompletionSource.Task;
	}
	
	private void BackButton_Click(object? sender, RoutedEventArgs e)
	{
		_parent.Children.Remove(this);
		_taskCompletionSource.TrySetResult();
	}

	private async void TrainRemove_Click(object? sender, RoutedEventArgs e)
	{
		Popup popup = new Popup("Are you sure you want to remove this train?");
		
		if (await popup.Show(RootGrid) != DialogResult.Yes)
			return;
	}
}