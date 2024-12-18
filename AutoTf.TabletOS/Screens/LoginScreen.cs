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

[ScreenIdentifier("LoginScreen")]
public class LoginScreen : Screen
{
	public LoginScreen(Logger logger) : base(logger)
	{
		Mat background = CvInvoke.Imread("Images/TabletOSStart.png");
		CvInvoke.CvtColor(background, background, ColorConversion.Bgr2Bgra);
		base.Background = background;
		AddEntity(new Rectangle(new Point(0, 0), new Size(1024, 15), 0, new ColorSettings(new MCvScalar(0, 0, 0, 51), -1)));
		AddEntity(new Text(() => DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), new FontSettings(new MCvScalar(255, 255, 255), 15), new Point(2, -1)));
		
		Mat fullSignal = CvInvoke.Imread("Images/FullSignalSign.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(fullSignal, fullSignal, ColorConversion.Bgr2Bgra);
		
		Mat noWifiSign = CvInvoke.Imread("Images/NoWifiSign.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(noWifiSign, noWifiSign, ColorConversion.Bgr2Bgra);
		
		AddEntity(new Image(fullSignal, new Size(15, 10), new Point(985, 4)));
		AddEntity(new Image(noWifiSign, new Size(17, 17), new Point(1005, 0)));
		
		List<EntityBase> exampleButton = new List<EntityBase>();
		
		Mat drawnTrain = CvInvoke.Imread("Images/DrawnTrain.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(noWifiSign, noWifiSign, ColorConversion.Bgr2Bgra);
		
		exampleButton.Add(new Rectangle(new Point(0, 0), new Size(438, 50), 0, new ColorSettings(new MCvScalar(0, 0, 0, 100), -1)));
		
		exampleButton.Add(new Circle(new Point(25, 25), 23, new ColorSettings(new MCvScalar(255, 255, 255, 100),-1)));
		exampleButton.Add(new Circle(new Point(25, 25), 23, new ColorSettings(new MCvScalar(0, 0, 0, 255), 1)));
		exampleButton.Add(new Image(drawnTrain, new Size(44, 34), new Point(12, 9)));
		exampleButton.Add(new Text(() => "Talent 2 - 461-037", new FontSettings(new MCvScalar(255, 255, 255), 22), new Point(52, 13)));
		
		Mat selectArrow = CvInvoke.Imread("Images/SelectArrow.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(selectArrow, selectArrow, ColorConversion.Bgr2Bgra);
		exampleButton.Add(new Image(selectArrow, new Size(22, 22), new Point(403, 14)));
		
		AddEntity(new ContentButton(() => exampleButton, new Point(60, 132), new Size(450, 45)));
	}

	public override void Mute()
	{
		
	}

	public override void Resume()
	{
	}
}