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
			AppDomain.CurrentDomain.ProcessExit += (sender, e) => DisposeExit(); 
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => DisposeExit(); 
			
			_screen = new StartScreen();
			Thread.Sleep(-1);
		}
		catch (Exception e)
		{
			Console.WriteLine("Root Error:");
			Console.WriteLine(e);
		}
	}

	private static void DisposeExit()
	{
		new Logger().Log("Unhandled Exception");
		_screen.Dispose();
	}
}