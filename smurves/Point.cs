using System.Diagnostics;

namespace cmdwtf.Smurves;


[DebuggerDisplay("X = {X}, Y = {Y}, Z = {Z}")]
public readonly record struct Point3(SmurveRaw X = 0, SmurveRaw Y = 0, SmurveRaw Z = 0)
{
	public static readonly Point3 Zero = new();

	public bool IsZero =>
		X == 0 &&
		Y == 0 &&
		Z == 0;
}


[DebuggerDisplay("X = {X}, Y = {Y}")]
public readonly record struct Point2(SmurveRaw X = 0, SmurveRaw Y = 0)
{
	public static readonly Point2 Zero = new();

	public bool IsZero =>
		X == 0 &&
		Y == 0;
}
