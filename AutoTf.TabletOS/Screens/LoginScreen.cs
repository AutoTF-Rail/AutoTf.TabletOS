using System.Drawing;
using AutoTf.Logging;
using AutoTf.Renderer;
using AutoTf.Renderer.Models.Entities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Image = AutoTf.Renderer.Models.Entities.Image;
using Rectangle = AutoTf.Renderer.Models.Entities.Rectangle;

namespace AutoTf.TabletOS.Screens;

public class LoginScreen : Screen
{
	public LoginScreen(Logger logger) : base(logger)
	{
		Mat background = CvInvoke.Imread("Assets/TabletOSStart.png");
		CvInvoke.CvtColor(background, background, ColorConversion.Bgr2Bgra);
		base.Background = background;
		AddEntity(new Rectangle(new Point(0, 0), new Size(1024, 15), new MCvScalar(0, 0, 0, 51), true));
		AddEntity(new Text(() => DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), .4f, FontFace.HersheySimplex, new Point(2, 11), new MCvScalar(255, 255, 255), 1));
		
		Mat fullSignal = CvInvoke.Imread("Assets/FullSignalSign.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(fullSignal, fullSignal, ColorConversion.Bgr2Bgra);
		
		Mat noWifiSign = CvInvoke.Imread("Assets/NoWifiSign.png", ImreadModes.Unchanged);
		CvInvoke.CvtColor(noWifiSign, noWifiSign, ColorConversion.Bgr2Bgra);
		
		AddEntity(new Image(fullSignal, new Size(15, 10), new Point(985, 4), new MCvScalar(255)));
		AddEntity(new Image(noWifiSign, new Size(17, 17), new Point(1005, 0), new MCvScalar(255)));
	}
}