namespace cmdwtf.Smurves;

public record SampleRange(SmurveComponent Start, SmurveComponent End) : Interval(Start, End)
{
	internal const SmurveComponent MinimumRange = 0.0;
	internal const SmurveComponent MaximumRange = 1.0;
	internal const SmurveComponent DefaultRangePadding = 0.1;
	internal const SmurveComponent DefaultRangeStart = MinimumRange + DefaultRangePadding;
	internal const SmurveComponent DefaultRangeEnd = MaximumRange - DefaultRangePadding;

	public static readonly SampleRange DefaultRange = new(DefaultRangeStart, DefaultRangeEnd);
}
