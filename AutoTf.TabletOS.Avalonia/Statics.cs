using System;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Controls;

namespace AutoTf.TabletOS.Avalonia;

public static class Statics
{
	public static Action? BrightnessChanged;
	public static double CurrentBrightness = 1.0;
	public static IDataManager DataManager;
	public static IProcessReader ProcessReader;
	public static Window Window;
}