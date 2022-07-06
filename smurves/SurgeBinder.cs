using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cmdwtf.Smurves;

public class SurgeBinder
{
	private SurgeBinderSettings Settings { get; init; }

	public List<Curve2> Output { get; } = new List<Curve2>();

	public event Action<decimal>? CurveProgress;

	public event Action<Curve2>? RejectedCurve;

	public uint CurveGenerationFailures { get; private set; } = 0;

	public SurgeBinder(SurgeBinderSettings settings)
	{
		Settings = settings;
		Settings.ValidateSettings();
	}

	//	public SurgeBinder(Interval intervalX, Interval intervalY, int samplesPerCurve,
	//		int directionFlipMaximum = 0, SmurveVector2? convergence = null, bool logScale = false, bool randomLaunch = false, bool rightConvergence = false,
	//		Interval? changeRange = null, uint changeSpacing = 1, SmurveComponent? changeRatio = null, SmurveComponent? startForce = null)
	//	{
	//		Settings = new()
	//		{
	//			IntervalX = intervalX,
	//			IntervalY = intervalY,
	//			CurveSampleCount = samplesPerCurve,
	//			DirectionFlipMaximum = directionFlipMaximum,
	//			Convergence = convergence ?? SmurveVector2.Zero,
	//			LogScale = logScale,
	//			RandomLaunch = randomLaunch,
	//			RightConvergence = rightConvergence,
	//			ChangeRange = changeRange ?? SampleRange.DefaultRange,
	//			ChangeSpacing = changeSpacing,
	//			ChangeRatio = changeRatio,
	//			StartForce = startForce,
	//		};
	//
	//		Settings.ValidateSettings();
	//	}

	public List<Curve2> Generate(int amount)
	{
		SmurvesRandom.Initialize();

		// calculate step size
		SmurveComponent sampleStepDelta = Settings.IntervalX.Distance / (Settings.CurveSampleCount - 1);

		// calculate measurement locations
		List<SmurveComponent> stepList = new();
		stepList.AddRange(
				Enumerable.Range(0, Settings.CurveSampleCount)
				.Select(index => Settings.IntervalX.Start + (index * sampleStepDelta))
			);

		IEnumerable<SmurveComponent> steps = stepList;

		bool flat_state;
		SmurveComponent? flat_value = null;

		if (!Settings.StartForce.HasValue)
		{
			flat_state = false;
			flat_value = null;
		}
		else
		{
			flat_state = true;

			if (Settings.LogScale)
			{
				// if for log-scale, recalculate the flat state ending
				IEnumerable<SmurveComponent> logSteps = LogarithmicGenerator(Settings.IntervalX, Settings.CurveSampleCount);
				SmurveComponent flatLogStart = Settings.StartForce.Value;
				//SmurveComponent logStartInverse = Settings.Logarithm(Settings.StartForce.Value);
				int logCut = logSteps.Where(s => s < flatLogStart).Count();
				flat_value = steps.ElementAt(logCut);
			}
			else
			{
				flat_value = Settings.StartForce.Value;
			}
		}

		Output.Clear();

		// generate curves
		Output.AddRange(CurveGenerator(amount, steps, sampleStepDelta, flat_state, flat_value));

		// preparing final output

		// Transform to log-scale measurements if required by the user
		if (Settings.LogScale)
		{
			// Save the log-scale measurement points into the curves
			steps = LogarithmicGenerator(Settings.IntervalX, Settings.CurveSampleCount);
			foreach (Curve2 curve in Output)
			{
				for (int stepScan = 0; stepScan < curve.Samples.Count; ++stepScan)
				{
					SmurveVector2 sample = curve.Samples[stepScan];
					sample = sample.SetX(steps.ElementAt(stepScan));
					curve.Samples[stepScan] = sample;
				}
			}
		}

		// If right-side convergence is requested, flip the values
		if (Settings.RightConvergence)
		{
			foreach (Curve2 curve in Output)
			{
				curve.Samples.Reverse();
			}
		}

		return Output;
	}


	private static readonly Interval LaunchAngleInterval = new(-90, 90);


	private IEnumerable<Curve2> CurveGenerator(int amount, IEnumerable<SmurveComponent> steps, SmurveComponent stepSize, bool flat_state, SmurveComponent? flat_value)
	{
		uint generated = 0;
		CurveGenerationFailures = 0;

		while (generated < amount)
		{
			SmurveComponent? startForce = null;
			int lowerRange;
			int flatChangeIndex = 0;
			SmurveComponent lowerStandard;
			int higherRange;
			int stepCount = steps.Count();

			bool hasConvergenceGiven = !Settings.Convergence.AlmostZero();

			// If no convergence point is given sample a random one
			SmurveVector2 convergencePoint = hasConvergenceGiven
				? Settings.Convergence
				: new(Settings.IntervalX.Start, Settings.IntervalY.Uniform);

			// Reset the start force if a flat state is requested
			if (flat_state)
			{
				startForce = flat_value;
			}

			//startForce = flatValue.GetValueOrDefault(StartForce.GetValueOrDefault(0));

			// Generate the random force direction change points
			IEnumerable<int>? allowed = Enumerable.Range(0, SmurvesRandom.Uniform(0, Settings.DirectionFlipMaximum + 1));

			if (!startForce.HasValue)
			{
				lowerRange = (int)(stepCount * Settings.ChangeRange.Start);
			}
			else
			{
				SmurveComponent diffA = startForce.Value - Settings.IntervalX.Start;
				SmurveComponent diffB = Settings.IntervalX.Distance;
				SmurveComponent ratio = diffA / diffB;
				flatChangeIndex = (int)(stepCount * ratio);
				lowerStandard = stepCount * Settings.ChangeRange.Start;
				lowerRange = (int)Math.Max(lowerStandard, flatChangeIndex);
			}

			higherRange = (int)(stepCount * Settings.ChangeRange.End);
			Range modifyRange = lowerRange..higherRange;
			int sampleNumber = SmurvesRandom.Uniform(0, Settings.DirectionFlipMaximum + 1);

			// Sample change points with the defined minimum space between
			List<int> changeIndexes = new();

			// Add the flat-start change point to the beginning
			if (startForce.HasValue)
			{
				// Adapt the change points to allow the flat start
				changeIndexes.Add(flatChangeIndex);
				sampleNumber++;
			}

			while (changeIndexes.Count < sampleNumber)
			{
				int changeIndex = SmurvesRandom.Sample(modifyRange);

				if (changeIndexes.TrueForAll(cp => Math.Abs(cp - changeIndex) >= Settings.ChangeSpacing))
				{
					changeIndexes.Add(changeIndex);
				}
			}

			changeIndexes.Sort();

			// Generate a random initial direction for the force
			Direction direction = SmurvesRandom.NextDirection();

			// Set the particle's velocity to an arbitrary value
			SmurveComponent velocity = 1.0;

			// Set the angle to zero for a left-side convergence
			SmurveComponent launchAngle = Settings.RandomLaunch
				? LaunchAngleInterval.Uniform * Constants.DegreesToRadians
				: 0.0;
			// Get the maximum force to stay within the intervals
			SmurveComponent restTime = Settings.IntervalX.End / velocity;
			SmurveComponent maxRange = (direction.IsDecreasing
				? Settings.IntervalY.End
				: Settings.IntervalY.Start)
					- convergencePoint.Y;

			SmurveComponent absMax = -Math.Abs(maxRange);

			SmurveComponent spread = (velocity * Math.Sin(launchAngle)) - absMax;
			SmurveComponent forceMax = (2 * spread) / (restTime * restTime);

			// Randomly sample the force depending on the maximum
			SmurveComponent force = SmurvesRandom.Uniform(0, forceMax);

			// Set the convergence point as the first start point
			SmurveVector2 startPoint = convergencePoint;

			// Initialize a curve path with one point and a counter
			List<SmurveVector2> curvePath = new List<SmurveVector2>();
			//curvePath.Add(startPoint);

			// Initialize the beginning as the last visited point
			SmurveVector2 lastPoint = startPoint;
			int offset = 0;
			SmurveComponent saveForce = 0;

			// Loop over change points to calculate partial curves
			while (curvePath.Count < stepCount - 1)
			{
				IList<SmurveComponent> partialSamples;

				// Set the steps depending on the process' status
				if (changeIndexes.Count == 0)
				{
					partialSamples = steps.Skip(offset).Take(stepCount - offset).ToList();
					offset = stepCount;
				}
				else
				{
					int nextIndex = changeIndexes.First();
					int nextTake = (nextIndex + 1) - offset;
					partialSamples = steps.Skip(offset).Take(nextTake).ToList();
					offset += nextTake - 1;
					changeIndexes.RemoveAt(0);

					// Sample a random force for the partial curve
					SmurveComponent scaleFactor = stepCount / (SmurveComponent)partialSamples.Count;
					forceMax *= scaleFactor;

					if (startForce.HasValue)
					{
						force = 0;
						startForce = null;
					}
					else
					{
						if (!Settings.ChangeRatio.HasValue || saveForce == 0)
						{
							force = SmurvesRandom.Uniform(0, forceMax);
						}
						else
						{
							SmurveComponent ratioProduct = saveForce * Settings.ChangeRatio.Value;
							SmurveComponent limiter = Math.Min(forceMax, ratioProduct);
							force = SmurvesRandom.Uniform(0, limiter);
						}
					}
				}

				// Save the force used to generate the partial curve
				saveForce = force;

				// Calculate the trajectory for the partial curve
				Trajectory t = CalculateTrajectory(force, velocity, direction, stepSize, startPoint, launchAngle, partialSamples);

				// Assign the values from the CalculateTrajectory
				Curve2 partialPath = t.Points;
				lastPoint = t.LastPoint ?? throw new InvalidDataException($"Expected a value for {nameof(t.LastPoint)}");
				launchAngle = t.ImpactAngle;
				velocity = t.Velocity;

				// Update parameters for the next loop iteration
				startPoint = lastPoint;
				direction = -direction;
				force *= -1;

				// Get the maximum force to stay within the intervals
				restTime = (Settings.IntervalX.End - lastPoint.X) / velocity;
				maxRange = (direction.IsDecreasing
					? Settings.IntervalY.End
					: Settings.IntervalY.Start)
						- lastPoint.Y;

				absMax = -Math.Abs(maxRange);

				spread = (velocity * Math.Sin(launchAngle)) - absMax;
				forceMax = (2 * spread) / (restTime * restTime); // #todo: what if i fall off a roof?

				// Randomly sample the force depending on the maximum
				if (!Settings.ChangeRatio.HasValue || Settings.ChangeRatio.Value == 0)
				{
					force = SmurvesRandom.Uniform(0, forceMax);
				}
				else
				{
					SmurveComponent limiter = Math.Min(saveForce, forceMax);
					force = SmurvesRandom.Uniform(0, limiter * Settings.ChangeRatio.Value);
				}

				// Append the partial path to the growing curve
				curvePath.AddRange(partialPath.Samples);
			}

			// add the curve's actual final point which would have
			// been skipped in the list returned from CalculateTrajectory
			curvePath.Add(lastPoint);

			// throw out the path if any trajectory is beyond the allowed range
			SmurveComponent minY = curvePath.Min(p => p.Y);
			SmurveComponent maxY = curvePath.Max(p => p.Y);
			bool lowFail = minY < Settings.IntervalY.Start && SmurveComponent.IsNormal(minY);
			bool highFail = maxY > Settings.IntervalY.End && SmurveComponent.IsNormal(maxY);
			if (lowFail || highFail || curvePath.Count != stepCount)
			{
				CurveGenerationFailures++;

				if (Settings.RaiseRejectedCurveEvent)
				{
					RejectedCurve?.Invoke(curvePath);
				}

				continue;
			}

			generated++;

			// Yield the computed curve as part of the collection
			yield return curvePath;

			decimal progressPercent = generated / amount;
			CurveProgress?.Invoke(progressPercent);

		}

		yield break;
	}

	private Trajectory CalculateTrajectory(SmurveComponent force, SmurveComponent velocity, Direction direction, SmurveComponent stepSize, SmurveVector2 startPoint, SmurveComponent launchAngle, IList<SmurveComponent> partialSteps)
	{
		// Initialize the horizontal displacement of the particle
		SmurveComponent horizontalDisplacement = 0;

		// Calculate the first horizontal and vertical velocities
		SmurveComponent horizontalVelocity = velocity * Math.Cos(launchAngle);
		SmurveComponent verticalVelocity = velocity * Math.Sin(launchAngle);

		// Save the initial velocity for consecutive calculations
		SmurveComponent startVelocity = velocity;

		// Initialize a list for storing the measurement points
		List<SmurveVector2> points = new();
		points.Add(startPoint);

		// Loop over the number of steps minus the initial step
		for (int scan = 1; scan < partialSteps.Count; ++scan)
		{
			// Get the horizontal distance, displacement and time
			SmurveComponent distance = partialSteps[scan];
			horizontalDisplacement += stepSize;
			SmurveComponent time = horizontalDisplacement / horizontalVelocity;

			// Calculate the vertical velocity and displacement
			SmurveComponent interim = startVelocity * Math.Sin(launchAngle);
			verticalVelocity = interim - (force * time);
			SmurveComponent velocityPart = startVelocity * (Math.Sin(launchAngle) * time);
			SmurveComponent forcePart = 0.5 * (force * (time * time));
			SmurveComponent verticalDisplacement = (velocityPart - forcePart) * 1;

			// Calculate the total velocity at the given point
			velocity = Math.Sqrt((horizontalVelocity * horizontalVelocity) +
									(verticalVelocity * verticalVelocity));

			// Append the current measurement point to the list
			SmurveComponent directionDisplacement = direction.Value * verticalDisplacement;
			points.Add(new SmurveVector2(distance, startPoint.Y + directionDisplacement));
		}

		if (points.Count == 0)
		{
			throw new InvalidOperationException($"We need to generate at least one point! How many steps are we aiming for: {partialSteps.Count}");
		}

		// Calculate both the final velocity and impact angle
		SmurveComponent finalVelocity = Math.Sqrt((horizontalVelocity * horizontalVelocity) +
									(verticalVelocity * verticalVelocity));

		SmurveComponent impactAngle = Math.Atan((verticalVelocity * -1) / horizontalVelocity);

		SmurveVector2? lastInSamples = points.Last();
		points.RemoveAt(points.Count - 1);

		// Return the points, last point, angle and velocity
		return new Trajectory
		{
			Points = points,
			LastPoint = lastInSamples,
			ImpactAngle = impactAngle,
			Velocity = velocity,
		};
	}


	private IEnumerable<SmurveComponent> LogarithmicGenerator(Interval interval, int stepCount)
	{
		Interval logInteval = new(Settings.Logarithm(interval.Start), Settings.Logarithm(interval.End));
		return Numpy.LogSpace(logInteval.Start, logInteval.End, stepCount);
	}
}
