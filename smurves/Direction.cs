using Microsoft.Toolkit.Diagnostics;

namespace cmdwtf.Smurves;

public struct Direction
{
	public SmurveComponent Value { get; }

	public bool IsIncreasing => Value > 0;
	public bool IsDecreasing => Value < 0;

	public bool IsValid => Value != 0;

	private Direction(SmurveComponent value)
	{
		Value = value;
	}

	public static readonly Direction Increasing = new(1);
	public static readonly Direction Decreasing = new(-1);

	public static Direction operator -(Direction inst)
	{
		return inst.Invert();
	}

	public Direction Invert()
	{
		Guard.IsTrue(IsValid, nameof(IsValid));
		return IsIncreasing
			? Decreasing
			: Increasing;
	}
}
