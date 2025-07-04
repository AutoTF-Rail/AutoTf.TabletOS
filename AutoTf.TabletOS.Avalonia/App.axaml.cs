using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Autofac;
using AutoTf.Logging;
using AutoTf.TabletOS.Avalonia.Extensions;
using AutoTf.TabletOS.Avalonia.UI.Controls.ViewModels;
using AutoTf.TabletOS.Avalonia.ViewModels;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;
using Avalonia.Markup.Xaml;
using AutoTf.TabletOS.Avalonia.Views;
using AutoTf.TabletOS.Avalonia.Views.Dialog;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using AutoTf.TabletOS.Services;
#if DEBUG
using AutoTf.TabletOS.Services.Fakes;
#endif
using Avalonia.Data.Core.Plugins;
using Statics = AutoTf.TabletOS.Models.Statics;
using TrainSelectionView = AutoTf.TabletOS.Avalonia.Views.TrainSelectionView;

namespace AutoTf.TabletOS.Avalonia;

public partial class App : Application
{
	public static IContainer? Container { get; private set; }
	
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}
	
	private void DisableAvaloniaDataAnnotationValidation()
	{
		// Get an array of plugins to remove
		DataAnnotationsValidationPlugin[] dataValidationPluginsToRemove =
			BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

		// remove each entry found
		foreach (var plugin in dataValidationPluginsToRemove)
		{
			BindingPlugins.DataValidators.Remove(plugin);
		}
	}
	
	public override void OnFrameworkInitializationCompleted()
	{
		ContainerBuilder builder = new ContainerBuilder();
		
		Register(builder);

		Container = builder.Build();
		
		IViewRouter router = Container.Resolve<IViewRouter>();
		
		router.NavigateTo<MainView>();
		
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = Container.Resolve<MainWindow>();
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
		{
			singleView.MainView = Container.Resolve<MainSingleWindow>();
		}
		
		if (Current!.ApplicationLifetime is IControlledApplicationLifetime controlledLifetime)
		{
			// Subscribe to the Exit event
			controlledLifetime.Exit += (_, _) => Statics.Shutdown?.Invoke();
		}
		
		base.OnFrameworkInitializationCompleted();
	}

	private void Register(ContainerBuilder builder)
	{
		builder.RegisterType<ViewRouter>().As<IViewRouter>().SingleInstance();
		builder.RegisterInstance(new Logger(true)).As<Logger>();
		
		builder.RegisterType<YubiKeySession>().AsSelf().SingleInstance();
		
		// Services
#if DEBUG
		builder.RegisterType<FakeTrainControlService>().As<ITrainControlService>().SingleInstance();
		builder.RegisterType<FakeTrainCameraService>().As<ITrainCameraService>().SingleInstance();
		builder.RegisterType<FakeTrainInformationService>().As<ITrainInformationService>().SingleInstance();
		builder.RegisterType<FakeAicService>().As<IAicService>().SingleInstance();
#else
		builder.RegisterType<TrainControlService>().As<ITrainControlService>().SingleInstance();
		builder.RegisterType<TrainCameraService>().As<ITrainCameraService>().SingleInstance();
		builder.RegisterType<TrainInformationService>().As<ITrainInformationService>().SingleInstance();
		builder.RegisterType<AicService>().As<IAicService>().SingleInstance();
#endif
		
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
		
		builder.RegisterType<AicInformationControlViewModel>().AsSelf().AsyncInit();
		builder.RegisterType<EasyControlControlViewModel>().AsSelf().AsyncInit();
		builder.RegisterType<TopBarViewModel>().AsSelf().AsyncInit();
		builder.RegisterType<TrainInformationControlViewModel>().AsSelf().AsyncInit();
		
		// Dialog Views
		builder.RegisterType<TrainInfoView>().AsSelf();
		builder.RegisterType<RemoteLogsViewer>().AsSelf();
		builder.RegisterType<TrainDateSetterView>().AsSelf();
		builder.RegisterType<TrainChainView>().AsSelf();
		builder.RegisterType<AicControlView>().AsSelf();
		builder.RegisterType<InfoScreen>().AsSelf();
		
		// DialogViewModels
		builder.RegisterType<TrainInfoViewModel>().AsSelf().AsyncInit();
		builder.RegisterType<AicControlViewModel>().AsSelf().AsyncInit();
		builder.RegisterType<RemoteLogsViewerViewModel>().AsSelf();
		builder.RegisterType<TrainDateSetterViewModel>().AsSelf();
		builder.RegisterType<TrainChainViewModel>().AsSelf();
		builder.RegisterType<InfoScreenViewModel>().AsSelf();
		
		// Wrappers
		builder.RegisterType<TrainInformation>().AsSelf().SingleInstance();
		builder.RegisterType<AicInformation>().AsSelf().SingleInstance();
		builder.RegisterType<TrainCameraInformation>().AsSelf().SingleInstance();
		

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
		{
			MainWindow mainWindow = new MainWindow();
			builder.RegisterInstance(mainWindow);
			builder.RegisterInstance(mainWindow).As<IUiControl>();
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime)
		{
			MainSingleWindow mainWindow = new MainSingleWindow();
			builder.RegisterInstance(mainWindow);
			builder.RegisterInstance(mainWindow).As<IUiControl>();
		}
	}
}