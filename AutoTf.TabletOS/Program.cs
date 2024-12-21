using AutoTf.Renderer;
using AutoTf.TabletOS;

internal class Program
{
	private static StartScreen _screen;
	public static void Main(string[] args)
	{
		try
		{
			AppDomain.CurrentDomain.ProcessExit += (sender, e) => _screen.Dispose(); 
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => _screen.Dispose(); 
			
			_screen = new StartScreen();
			Thread.Sleep(-1);
		}
		catch (Exception e)
		{
			Console.WriteLine("Root Error:");
			Console.WriteLine(e);
		}
	}
}