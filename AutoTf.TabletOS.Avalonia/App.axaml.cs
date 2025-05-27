using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Autofac;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using Avalonia.Markup.Xaml;
using AutoTf.TabletOS.Avalonia.Views;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
using AutoTf.TabletOS.Services.Fakes;
using Statics = AutoTf.TabletOS.Models.Statics;
using TrainSelectionView = AutoTf.TabletOS.Avalonia.Views.TrainSelectionView;

namespace AutoTf.TabletOS.Avalonia;

public class App : Application
{
	public static IContainer? Container { get; private set; }
	public static MainWindow? MainWindow { get; private set; }
	
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}
	
	public override void OnFrameworkInitializationCompleted()
	{
		ContainerBuilder builder = new ContainerBuilder();

		builder.RegisterType<ViewRouter>().As<IViewRouter>().SingleInstance();
		builder.RegisterInstance(new Logger(true)).As<Logger>();
		
		builder.RegisterType<YubiKeySession>().AsSelf().SingleInstance();
		
		// Services
		builder.RegisterType<FakeTrainControlService>().As<ITrainControlService>().SingleInstance();
		builder.RegisterType<FakeTrainCameraService>().As<ITrainCameraService>().SingleInstance();
		builder.RegisterType<FakeTrainInformationService>().As<ITrainInformationService>().SingleInstance();
		builder.RegisterType<FakeAicService>().As<IAicService>().SingleInstance();
		
		builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();
		builder.RegisterType<NetworkService>().As<INetworkService>().SingleInstance();
		
		// Views
		builder.RegisterType<MainView>().AsSelf();
		builder.RegisterType<TrainSelectionView>().AsSelf();
		builder.RegisterType<TrainControlView>().AsSelf();
		
		// ViewModels
		builder.RegisterType<MainViewViewModel>().AsSelf();
		builder.RegisterType<TrainSelectionViewModel>().AsSelf();
		builder.RegisterType<TrainControlViewModel>().AsSelf();
		builder.RegisterType<TrainInformationControlViewModel>().AsSelf();
		
		// Dialog Views
		builder.RegisterType<TrainInfoView>().AsSelf();
		builder.RegisterType<RemoteLogsViewer>().AsSelf();
		builder.RegisterType<TrainDateSetterView>().AsSelf();
		builder.RegisterType<TrainChainView>().AsSelf();
		builder.RegisterType<AicControlView>().AsSelf();
		builder.RegisterType<InfoScreen>().AsSelf();
		
		// DialogViewModels
		builder.RegisterType<TrainInfoViewModel>().AsSelf();
		builder.RegisterType<RemoteLogsViewerViewModel>().AsSelf();
		builder.RegisterType<TrainDateSetterViewModel>().AsSelf();
		builder.RegisterType<TrainChainViewModel>().AsSelf();
		builder.RegisterType<AicInformationControlViewModel>().AsSelf();
		builder.RegisterType<AicControlViewModel>().AsSelf();
		builder.RegisterType<EasyControlControlViewModel>().AsSelf();
		builder.RegisterType<TopBarViewModel>().AsSelf();
		builder.RegisterType<InfoScreenViewModel>().AsSelf();
		
		// Wrappers
		builder.RegisterType<TrainInformation>().AsSelf().SingleInstance();
		builder.RegisterType<AicInformation>().AsSelf().SingleInstance();
		builder.RegisterType<TrainCameraInformation>().AsSelf().SingleInstance();

		MainWindow = new MainWindow();
		Container = builder.Build();
		
		IViewRouter router = Container.Resolve<IViewRouter>();
		
		router.NavigateTo<MainView>();
		
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = MainWindow;
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
		{
			singleView.MainView = MainWindow;
		}
		
		base.OnFrameworkInitializationCompleted();
		
		if (Current!.ApplicationLifetime is IControlledApplicationLifetime controlledLifetime)
		{
			// Subscribe to the Exit event
			controlledLifetime.Exit += (_, _) => Statics.Shutdown?.Invoke();
		}
	}
}