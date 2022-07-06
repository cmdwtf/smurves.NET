using System;

namespace cmdwtf.Smurves;

public static class Constants
{
	public const SmurveComponent DegreesToRadians = (Math.PI / 180.0);
	public static Func<SmurveComponent, SmurveComponent> DefaultLogarithm = Math.Log10;
	public static Func<SmurveComponent, SmurveComponent> DefaultInverseLogarithm = v => Math.Pow(10, v);
}
