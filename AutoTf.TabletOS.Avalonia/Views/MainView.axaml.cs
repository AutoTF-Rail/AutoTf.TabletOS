using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class MainView : UserControl
{
	public MainView()
	{
		InitializeComponent();
	
		StaticEvents.BrightnessChanged += BrightnessChanged;
	}

	private void BrightnessChanged()
	{
		this.Opacity = StaticEvents.CurrentBrightness;
	}
}