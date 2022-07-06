
using System;

using RandN;
using RandN.Distributions;

using RDist = RandN.Distributions;

namespace cmdwtf.Smurves;

internal class SmurvesRandom
{
	internal static IRng Random { get; private set; }

	private static IDistribution<bool> FiftyFifty { get; } = Bernoulli.FromP(0.5);

	static SmurvesRandom()
	{
		Random = StandardRng.Create();
		Initialize();
	}

	public static int Uniform(int lowerInclusive, int upperExclusive)
	{
		Uniform.Int32 i32 = lowerInclusive < upperExclusive
			? RDist.Uniform.New(lowerInclusive, upperExclusive)
			: RDist.Uniform.New(upperExclusive, lowerInclusive);

		return i32.Sample(Random);
	}

	public static SmurveComponent Uniform(SmurveComponent lowerInclusive, SmurveComponent upperInclusive)
	{
		if (lowerInclusive == upperInclusive)
		{
			return lowerInclusive;
		}

		if (upperInclusive == SmurveComponent.PositiveInfinity)
		{
			upperInclusive = SmurveComponent.MaxValue;
		}

		if (lowerInclusive == SmurveComponent.PositiveInfinity)
		{
			lowerInclusive = SmurveComponent.MaxValue;
		}

		if (upperInclusive == SmurveComponent.NegativeInfinity)
		{
			upperInclusive = SmurveComponent.MinValue;
		}

		if (lowerInclusive == SmurveComponent.NegativeInfinity)
		{
			lowerInclusive = SmurveComponent.MinValue;
		}

		Uniform.Double fUni = lowerInclusive < upperInclusive
			? RDist.Uniform.Double.CreateInclusive(lowerInclusive, upperInclusive)
			: RDist.Uniform.Double.CreateInclusive(upperInclusive, lowerInclusive);
		return fUni.Sample(Random);
	}

	public static int Sample(Range range) => Uniform(range.Start.Value, range.End.Value + 1);

	public static Direction NextDirection()
		=> FiftyFifty.Sample(Random)
			? Direction.Increasing
			: Direction.Decreasing;

	public static SmurveComponent NextPositiveOrNegative()
		=> FiftyFifty.Sample(Random)
			? 1.0
			: -1.0;
	internal static void Initialize()
	{
#if !DEBUG
		System.Diagnostics.Debug.WriteLine("Seeing random with fixed seed and seeking a default position.");
		var seed = new RandN.Rngs.ChaCha.Seed(new uint[8]);
		var chaCha = RandN.Rngs.ChaCha.Create(seed);
		chaCha.Position = new RandN.Rngs.ChaCha.Counter(0, 0);
		Random = chaCha;
#endif // DEBUG
	}
}
