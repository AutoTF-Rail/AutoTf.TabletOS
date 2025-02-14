using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using TaskCompletionSource = System.Threading.Tasks.TaskCompletionSource;

namespace AutoTf.TabletOS.Avalonia.Views;

public partial class EasyControlView : UserControl
{
	private TaskCompletionSource _taskCompletionSource = null!;
	private Grid _parent = null!;
	
	public EasyControlView()
	{
		InitializeComponent();
		ZeroStep.IsVisible = false;
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

	private void SpeedSlider_Change(object? sender, RangeBaseValueChangedEventArgs e)
	{
		if (SpeedSlider == null)
			return;
		
		int row = (100 - int.Parse(SpeedSlider.Value.ToString(CultureInfo.InvariantCulture))) / 5;
		
		int visual = 100 - row * 200 / 20;

		if (row != 10)
		{
			(int red, int green, int blue) color = CalculateColorBasedOnVisual(visual);

			Color backgroundColor = Color.FromArgb(255, (byte)color.red, (byte)color.green, (byte)color.blue);

			StepColor.Background = new SolidColorBrush(backgroundColor);
		}
		else 
			StepColor.Background = Brushes.Gray;

		PlusFinalStep.IsVisible = row != 0;

		MinusFinalStep.IsVisible = row != 20;

		ZeroStep.IsVisible = row != 10;
		
		Grid.SetRow(SelectedStep, row);
		SelectedStepValue.Text = visual + "%";
		// Grid.SetRowSpan(SelectedStep, 0);
		// Grid.SetRow();
		// SelectedStep.Parent
		// value == 100 : top : Row 0
		// value == 50 : middle : Row 10
		// value = 0 : bottom : Row 20
	}
	
	public (int red, int green, int blue) CalculateColorBasedOnVisual(int visual)
	{
		int red;
		int green;

		if (visual > 0)
		{
			green = (visual * 127 / 100) + 128; 

			red = 128 - (visual * 128 / 100); 
		}
		else if (visual < 0)
		{
			red = (-visual * 127 / 100) + 128; 
			
			green = 128 - (-visual * 128 / 100); 
		}
		else
		{
			red = 128;
			green = 128;
		}

		int blue = 0;

		return (red, green, blue);
	}
}