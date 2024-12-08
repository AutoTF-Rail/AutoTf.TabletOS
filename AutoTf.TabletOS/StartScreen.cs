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

	public StartScreen()
	{
		_render = new RenderEngine(_logger, true, 1024, 600, true, true);

		Mat background =
			CvInvoke.Imread(
				"/home/display/AutoTf.TabletOS/AutoTf.TabletOS/bin/Debug/net8.0/Images/TabletOSLogin.png");
		CvInvoke.CvtColor(background, background, ColorConversion.Bgr2Bgra);
		_render.SetBackgroundImage(background);
		_render.AddButton(new Button("Shutdown", .4f, FontFace.HersheyScriptSimplex, new Point(5, 5), new Size(50, 15),
			new MCvScalar(100, 100, 100)));
		// _render.AddText(new Text("Welcome to TabletOS", .4f, FontFace.HersheyScriptSimplex, new Point(512, 300),
		// 	new MCvScalar(0), 1));
	}

	public void Dispose()
	{
		_render.Dispose();
		_logger.Dispose();
	}
}