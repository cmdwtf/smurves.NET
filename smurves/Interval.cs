
using System;
using System.Diagnostics;

using Microsoft.Toolkit.Diagnostics;

namespace cmdwtf.Smurves;


[DebuggerDisplay(@"\{{Start}..{End}\}")]
public record Interval(SmurveComponent Start = 0, SmurveComponent End = 0)
{
	public static readonly Interval Zero = new();

	public SmurveComponent Distance
	{
		get
		{
			Guard.IsLessThan(Start, End, nameof(Start));
			return Math.Abs(End - Start);
		}
	}

	public SmurveComponent Uniform
	{
		get
		{
			Guard.IsLessThan(Start, End, nameof(Start));
			return SmurvesRandom.Uniform(Start, End);
		}
	}

	public SmurveComponent Lerp(SmurveComponent t)
	{
		Guard.IsLessThan(Start, End, nameof(Start));
		return (t * End) + ((1 - t) * Start);
	}
}
