using System.Collections.Generic;

namespace cmdwtf.Smurves;

public record Trajectory
{
	//public Curve Path { get; } = new(new List<Point>());
	public List<SmurveVector2> Points { get; init; } = new();
	public SmurveVector2? LastPoint { get; init; } = SmurveVector2.Zero;
	public double ImpactAngle { get; init; }
	public double Velocity { get; init; }
}
