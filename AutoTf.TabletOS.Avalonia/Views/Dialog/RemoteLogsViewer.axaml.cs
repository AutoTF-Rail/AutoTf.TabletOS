using AutoTf.TabletOS.Avalonia.ViewModels.Base;
using AutoTf.TabletOS.Avalonia.ViewModels.Dialog;

namespace AutoTf.TabletOS.Avalonia.Views.Dialog;

public partial class RemoteLogsViewer : ViewBase<RemoteLogsViewerViewModel>
{
	public RemoteLogsViewer()
	{
		InitializeComponent();
	}

	public RemoteLogsViewer(RemoteLogsViewerViewModel vm) : base(vm)
	{
		InitializeComponent();
	}
}