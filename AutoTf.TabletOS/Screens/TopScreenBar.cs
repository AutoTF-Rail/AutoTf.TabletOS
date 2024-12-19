using System.Drawing;
using AutoTf.Logging;
using AutoTf.Renderer;
using AutoTf.Renderer.Models;
using AutoTf.Renderer.Models.Entities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Image = AutoTf.Renderer.Models.Entities.Image;
using Rectangle = AutoTf.Renderer.Models.Entities.Rectangle;

namespace AutoTf.TabletOS.Screens;

[ScreenIdentifier("TopScreenBar")]
public class TopScreenBar : Screen
{
	private readonly IRenderBase _render;
	private readonly Logger _logger;
	
	public TopScreenBar(IRenderBase render, Logger logger) : base(logger)
	{
		try
		{
			_render = render;
			_logger = logger;
		
			AddEntity(new Rectangle(new Point(0, 0), new Size(1024, 15), 0, new ColorSettings(new MCvScalar(0, 0, 0, 51), -1), ShowQuickActionsMenu));
			AddEntity(new Text(() => DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), new FontSettings(new MCvScalar(255, 255, 255), 15), new Point(2, -1)));
		
			Mat fullSignal = CvInvoke.Imread("Images/FullSignalSign.png", ImreadModes.Unchanged);
			CvInvoke.CvtColor(fullSignal, fullSignal, ColorConversion.Bgr2Bgra);
		
			Mat noWifiSign = CvInvoke.Imread("Images/NoWifiSign.png", ImreadModes.Unchanged);
			CvInvoke.CvtColor(noWifiSign, noWifiSign, ColorConversion.Bgr2Bgra);
		
			AddEntity(new Image(fullSignal, new Size(15, 10), new Point(985, 4)));
			AddEntity(new Image(noWifiSign, new Size(17, 17), new Point(1005, 0)));
		}
		catch (Exception e)
		{
			logger.Log(e.Message);
		}
	}

	private void ShowQuickActionsMenu()
	{
		QuickActionsMenu quickShowMenu = new QuickActionsMenu(_render, _logger);
		quickShowMenu.Show();
	}
	
	public override void Mute()
	{
	}

	public override void Resume()
	{
	}
}