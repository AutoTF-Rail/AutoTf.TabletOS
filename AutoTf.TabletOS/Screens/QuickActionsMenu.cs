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

[ScreenIdentifier("QuickShowMenu")]
public class QuickActionsMenu : Screen
{
	private readonly IRenderBase _render;
	private IScreen _previousScreen;
	private bool isShown = false;
	private readonly Action CloseEvent;
	
	public QuickActionsMenu(Action closeEvent, IRenderBase render, Logger logger) : base(logger)
	{
		CloseEvent = closeEvent;
		_render = render;
		
		AddEntity(new Rectangle(new Point(0, 0), new Size(render.RenderSize.Width, render.RenderSize.Height), 0, new ColorSettings(new MCvScalar(20, 20, 20, 150), -1), Close));

		AddEntity(new Rectangle(new Point(625, 21), new Size(389, 274), 0,
			new ColorSettings(new MCvScalar(0, 0, 0, 125), -1)));
		AddEntity(new Rectangle(new Point(625, 21), new Size(389, 274), 0,
			new ColorSettings(new MCvScalar(255, 255, 255, 255), 1)));
		//AddEntity(new Text(() => "Quick Actions", new FontSettings(new MCvScalar(255, 255, 255), 15),
		//	new Point(775, 30)));
		
		Mat darkerIcon = CvInvoke.Imread("Images/DarkerIcon.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(darkerIcon, darkerIcon, ColorConversion.Bgr2Bgra);
		
		AddEntity(new ContentButton(() => new List<EntityBase>()
		{
			new Rectangle(new Point(0, 0), new Size(50, 50), 0,
				new ColorSettings(new MCvScalar(200, 200, 200, 255), -1)),
			new Rectangle(new Point(0, 0), new Size(50, 50), 0, new ColorSettings(new MCvScalar(0, 0, 0, 255), 1)),
			new Image(darkerIcon, new Size(35, 35), new Point(8, 8)),
		}, new Point(640, 70), new Size(50, 50)));
		
		Mat brighterIcon = CvInvoke.Imread("Images/BrighterIcon.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(brighterIcon, brighterIcon, ColorConversion.Bgr2Bgra);
		
		AddEntity(new ContentButton(() => new List<EntityBase>()
		{
			new Rectangle(new Point(0, 0), new Size(50, 50), 0,
				new ColorSettings(new MCvScalar(200, 200, 200, 255), -1)),
			new Rectangle(new Point(0, 0), new Size(50, 50), 0, new ColorSettings(new MCvScalar(0, 0, 0, 255), 1)),
			new Image(brighterIcon, new Size(35, 35), new Point(8, 8)),
		}, new Point(700, 70), new Size(50, 50)));
	}

	public void Show()
	{
		try
		{
			Mat bck = _render.RenderRaw(true, false);
			bck.ConvertTo(bck, DepthType.Cv8U);
			CvInvoke.CvtColor(bck, bck, ColorConversion.Bgr2Bgra);
			Background = bck;
			_render.SwapScreen(this);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	public void Close()
	{
		_render.SwapBackScreen();
		CloseEvent.Invoke();
	}
	
	public override void Mute()
	{
	}

	public override void Resume()
	{
	}
}