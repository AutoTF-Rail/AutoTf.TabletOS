using System;
using AutoTf.TabletOS.Models.Interfaces;

namespace AutoTf.TabletOS.Avalonia;

public static class Statics
{
	public static Action? BrightnessChanged;
	public static double CurrentBrightness = 1.0;
	public static IDataManager DataManager;
	public static IProcessReader ProcessReader;
}