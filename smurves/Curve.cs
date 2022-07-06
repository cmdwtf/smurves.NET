using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace cmdwtf.Smurves;

[DebuggerDisplay("{Samples.Count} Sample Curve")]
public record Curve<T>(List<T> Samples) : ICurve where T : IEquatable<T>, IComparable<T>
{
	public Curve() : this(new List<T>())
	{ }

	public Curve(T singlePoint) : this(new List<T>() { singlePoint })
	{ }

	public static implicit operator Curve<T>(List<T> points)
	{
		return new Curve<T>(points);
	}
}

public record Curve2(List<SmurveVector2> Samples) : Curve<SmurveVector2>(Samples)
{
	public static readonly Curve2 Empty = new(new List<SmurveVector2>());

	public static implicit operator Curve2(List<SmurveVector2> points)
	{
		return new Curve2(points);
	}

	//public SmurveVector2 Minimum => new(Samples.Select(p => p.X).Min(), Samples.Select(p => p.Y).Min());
	//public SmurveVector2 Maximum => new(Samples.Select(p => p.X).Max(), Samples.Select(p => p.Y).Max());
}

public record Curve3(List<SmurveVector3> Samples) : Curve<SmurveVector3>(Samples)
{
	public static readonly Curve3 Empty = new(new List<SmurveVector3>());

	public static implicit operator Curve3(List<SmurveVector3> points)
	{
		return new Curve3(points);
	}
}
