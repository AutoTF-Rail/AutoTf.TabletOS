using System;

namespace AutoTf.TabletOS.Avalonia;

public static class StaticEvents
{
	public static Action? BrightnessChanged;
	public static double CurrentBrightness = 1.0;
}