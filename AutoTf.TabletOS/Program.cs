using AutoTf.TabletOS;

internal class Program
{
	public static void Main(string[] args)
	{
		try
		{
			StartScreen screen = new StartScreen();
			Thread.Sleep(-1);
		}
		catch (Exception e)
		{
			Console.WriteLine("Root Error:");
			Console.WriteLine(e);
		}
	}
}