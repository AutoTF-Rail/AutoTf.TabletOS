using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Services;
using AutoTf.TabletOS.Services.Fakes;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Preview;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		Statics.DataManager = new FakeDataManager();
		Statics.TrainInformationService = new FakeTrainInfo();
		Statics.TrainControlService = new FakeTrainControlService();
		Statics.TrainCameraService = new FakeTrainCameraService();
		Statics.AicService = new FakeAicService();
		
		DataContext = new MainWindowViewModel();
		InitializeComponent();
	}
}