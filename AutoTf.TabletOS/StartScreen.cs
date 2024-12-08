using System.Drawing;
using AutoTf.Logging;
using AutoTf.Renderer;
using AutoTf.Renderer.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AutoTf.TabletOS;

public class StartScreen : IDisposable
{
	private readonly RenderEngine _render;
	private readonly Logger _logger = new Logger();
	private int it = 0;

	public StartScreen()
	{
		_render = new RenderEngine(_logger, true, 1024, 600, true, true);

		Mat background =
			CvInvoke.Imread(
				"/home/display/AutoTf.TabletOS/AutoTf.TabletOS/bin/Debug/net8.0/Images/TabletOSLogin.png");
		CvInvoke.CvtColor(background, background, ColorConversion.Bgr2Bgra);
		_render.SetBackgroundImage(background);
		_render.AddButton(new Button("Shutdown", .6f, FontFace.HersheySimplex, new Point(15, 15), new Size(100, 30),
			new MCvScalar(0), new MCvScalar(10, 10, 10), true, new MCvScalar(255,255,255), OnClick));
		// _render.AddText(new Text("Welcome to TabletOS", .4f, FontFace.HersheyScriptSimplex, new Point(512, 300),
		// 	new MCvScalar(0), 1));
	}

	private void OnClick()
	{
		_render.AddText(new Text("BUTTON CLICK", 1f, FontFace.HersheySimplex, new Point(100, 100),
			new MCvScalar(255, it * 50, 0), 2));
		it++;
	}

	public void Dispose()
	{
		_render.Dispose();
		_logger.Dispose();
	}
}