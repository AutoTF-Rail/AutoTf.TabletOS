using System.Drawing;
using AutoTf.Logging;
using AutoTf.Renderer;
using AutoTf.Renderer.Models;
using AutoTf.TabletOS.Screens;

namespace AutoTf.TabletOS;

public class StartScreen : IDisposable
{
	private readonly IRenderBase _render;
	private readonly Logger _logger = new Logger();

	public StartScreen()
	{
		try
		{
			_render = new RenderBase(_logger, true, new Size(1024, 1024), new Size(1024, 600), true, true);
		
			_render.ActiveScreen = new LoginScreen(_render, _logger);
			_render.ActiveToolbar = new TopScreenBar(_render, _logger);
		}
		catch (Exception e)
		{
			_logger.Log(e.Message);
		}
	}

	public void Dispose()
	{
		_render.Dispose();
		_logger.Dispose();
	}
}