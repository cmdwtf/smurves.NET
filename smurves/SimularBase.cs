namespace cmdwtf.Smurves;

public abstract class SimularBase<T> : ISimular<T> where T : struct
{
	public T Origin { get; set; }

	public T Attitude { get; set; }

	public T Velocity { get; set; }

	public T Position { get; set; }

	public abstract T Step();
}
