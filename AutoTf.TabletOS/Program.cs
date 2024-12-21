using AutoTf.Logging;
using AutoTf.Renderer;
using AutoTf.TabletOS;

internal class Program
{
	private static StartScreen _screen;
	public static void Main(string[] args)
	{
		try
		{
			AppDomain.CurrentDomain.ProcessExit += (sender, e) => DisposeExit(null); 
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => DisposeExit(e); 
			
			_screen = new StartScreen();
			Thread.Sleep(-1);
		}
		catch (Exception e)
		{
			Console.WriteLine("Root Error:");
			Console.WriteLine(e);
		}
	}

	private static void DisposeExit(UnhandledExceptionEventArgs? e)
	{
		if (e != null)
		{
			Exception ex = (Exception)e.ExceptionObject;
			Logger logger = new Logger();
			logger.Log("Unhandled Root Exception");
			logger.Log(ex.Message);
			logger.Log(ex.StackTrace!);
			logger.Log(ex.InnerException?.Message!);
		}

		_screen.Dispose();
	}
}