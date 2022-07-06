using System;

using Microsoft.Toolkit.Diagnostics;

namespace cmdwtf.Smurves;

public class SurgeBinderSettings
{
	public const int DefaultCurveSampleCount = 100;
	public const int DefaultDirectionFlip = 3;
	public const int MinimumDirectionFlips = 1;

	public Interval IntervalX { get; init; } = Interval.Zero;
	public Interval IntervalY { get; init; } = Interval.Zero;
	public int CurveSampleCount { get; init; } = DefaultCurveSampleCount;
	public int DirectionFlipMaximum { get; init; } = DefaultDirectionFlip;
	public SmurveVector2 Convergence { get; init; } = SmurveVector2.Zero;
	public bool LogScale { get; init; }
	public bool RandomLaunch { get; init; }
	public bool RightConvergence { get; init; }
	public SampleRange ChangeRange { get; init; } = SampleRange.DefaultRange;
	public uint ChangeSpacing { get; init; }
	public SmurveComponent? ChangeRatio { get; init; }
	public SmurveComponent? StartForce { get; init; }

	public bool RaiseRejectedCurveEvent { get; init; } = false;

	public Func<SmurveComponent, SmurveComponent> Logarithm { get; init; } = Constants.DefaultLogarithm;
	public Func<SmurveComponent, SmurveComponent> InverseLogarithm { get; init; } = Constants.DefaultInverseLogarithm;

	internal void ValidateSettings()
	{
		Guard.IsLessThan(IntervalX.Start, IntervalX.End, nameof(IntervalX));
		Guard.IsLessThan(IntervalY.Start, IntervalY.End, nameof(IntervalX));
		Guard.IsGreaterThanOrEqualTo(DirectionFlipMaximum, MinimumDirectionFlips, nameof(DirectionFlipMaximum));

		if (!Convergence.AlmostZero())
		{
			Guard.IsEqualTo(Convergence.X, IntervalX.Start, nameof(Convergence.X));
		}

		Guard.IsBetweenOrEqualTo(ChangeRange.Start, SampleRange.MinimumRange, SampleRange.MaximumRange, nameof(ChangeRange.Start));
		Guard.IsBetweenOrEqualTo(ChangeRange.End, SampleRange.MinimumRange, SampleRange.MaximumRange, nameof(ChangeRange.End));

		double flipCount = CurveSampleCount / DirectionFlipMaximum;
		Guard.IsLessThan(ChangeSpacing, flipCount, nameof(ChangeSpacing));
		Guard.IsGreaterThanOrEqualTo(ChangeSpacing, 0, nameof(ChangeSpacing));

		if (ChangeRatio.HasValue)
		{
			Guard.IsGreaterThan(ChangeRatio.Value, 0, nameof(ChangeRatio));
		}

		if (StartForce.HasValue)
		{
			Guard.IsBetweenOrEqualTo(StartForce.Value, IntervalX.Start, IntervalX.End, nameof(StartForce));
		}

		if (LogScale)
		{
			Guard.IsTrue(IsInt(Logarithm(IntervalX.Start)), $"{nameof(IntervalX)}.{nameof(IntervalX.Start)} is valid log-scale value.");
			Guard.IsTrue(IsInt(Logarithm(IntervalX.End)), $"{nameof(IntervalX)}.{nameof(IntervalX.End)} is valid log-scale value.");

			if (!Convergence.AlmostZero())
			{
				Guard.IsTrue(IsInt(Logarithm(Convergence.X)), $"{nameof(Convergence)}.{nameof(Convergence.X)} is valid log-scale value.");
			}
		}

		static bool IsInt(double d) => Math.Abs(d % 1) <= (double.Epsilon * 100);
	}
}
