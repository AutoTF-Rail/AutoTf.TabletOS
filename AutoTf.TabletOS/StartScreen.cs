using System.Drawing;
using AutoTf.Logging;
using AutoTf.Renderer;
using AutoTf.Renderer.Models;
using AutoTf.TabletOS.Screens;

namespace AutoTf.TabletOS;

[ScreenIdentifier("StartScreen")]
public class StartScreen : IDisposable
{
	private readonly IRenderBase _render;
	private readonly Logger _logger = new Logger(true);

	public StartScreen()
	{
		// _render = new RenderEngine(_logger, true, 1024, 600, true, true);
		try
		{
			_logger.Log("Guh");
			_render = new RenderBase(_logger, true, new Size(1024, 600), new Size(1024, 600), false, true);
		
			_render.ActiveToolbar = new TopScreenBar(_render, _logger);

			_render.ActiveScreen = new LoginScreen(_logger);
		}
		catch (Exception e)
		{
			_logger.Log(e.Message);
			throw;
		}

		// Mat background =
		// CvInvoke.Imread(
		// "/home/display/AutoTf.TabletOS/AutoTf.TabletOS/bin/Debug/net8.0/Images/TabletOSLogin.png");
		// CvInvoke.CvtColor(background, background, ColorConversion.Bgr2Bgra);
		// _render.SetBackgroundImage(background);
		// _render.AddButton(new Button("Shutdown", .6f, FontFace.HersheySimplex, new Point(15, 15), new Size(125, 30),
		// 	new MCvScalar(0), new MCvScalar(10, 10, 10), true, new MCvScalar(255,255,255), OnClick));
		// _render.AddButton(new Button("Lighter", .6f, FontFace.HersheySimplex, new Point(15, 50), new Size(125, 30),
		// 	new MCvScalar(0), new MCvScalar(10, 10, 10), true, new MCvScalar(255,255,255), _render.HigherBrightness));
		// _render.AddButton(new Button("Darker", .6f, FontFace.HersheySimplex, new Point(15, 85), new Size(125, 30),
		// 	new MCvScalar(0), new MCvScalar(10, 10, 10), true, new MCvScalar(255,255,255), _render.LowerBrightness));
		// _render.AddText(new Text("Welcome to TabletOS", .4f, FontFace.HersheyScriptSimplex, new Point(512, 300),
		// 	new MCvScalar(0), 1));
	}

	// private void OnClick()
	// {
	// 	_render.AddPopup(new Popup(_render, "Guh???", () => _logger.Log("Clicked yes"), () => _logger.Log("Clicked no")));
	// 	_render.AddText(new Text("BUTTON CLICK", 1f, FontFace.HersheySimplex, new Point(100, 100),
	// 		new MCvScalar(255, it * 50, 0), 2));
	// 	it++;
	// }

	public void Dispose()
	{
		_render.Dispose();
		_logger.Dispose();
	}
}