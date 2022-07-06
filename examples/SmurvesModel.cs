using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using SWPoint = System.Windows.Point;

namespace cmdwtf.Smurves.Example;

internal class SurgeBinderModel : IModel
{
	/// <summary>
	/// Gets the plot model.
	/// </summary>
	public PlotModel Model { get; private set; }

	public SurgeBinderModel(bool logarithmicData, bool logarithmicPlot, bool plotRejects)
	{
		int lines = 10;

		SurgeBinderSettings settingsLog = new SurgeBinderSettings()
		{
			IntervalX = new(0.001, 10),
			IntervalY = new(0, 5),
			Convergence = new(0.001, 1.0),
			LogScale = true,
			ChangeRange = new(0.2, 0.8),
			StartForce = 0.01,
			RaiseRejectedCurveEvent = plotRejects,
		};

		SurgeBinderSettings settingsRegular = new SurgeBinderSettings()
		{
			IntervalX = new(0.5, 5),
			IntervalY = new(0, 2),
			Convergence = new(0.5, 1.0),
			ChangeRange = new(0.2, 0.8),
			StartForce = 0.5,
			RaiseRejectedCurveEvent = plotRejects,
		};

		SurgeBinderSettings settings = logarithmicData ? settingsLog : settingsRegular;


		PlotModel plot = new()
		{
			Title = "Smurves",
			Subtitle = logarithmicData ? "Logarithmic" : "Regular",
		};

		Axis xAxis =
			logarithmicPlot
			? new LogarithmicAxis
			{
				Position = AxisPosition.Bottom,
				Minimum = settings.IntervalX.Start,
				Maximum = settings.IntervalX.End,
				MajorStep = 1,
				MinorStep = 0.25,
				TickStyle = TickStyle.Inside,
			}
			: new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Minimum = settings.IntervalX.Start,
				Maximum = settings.IntervalX.End,
				MajorStep = 1,
				MinorStep = 0.25,
				TickStyle = TickStyle.Inside,
			};

		Axis yAxis = new LinearAxis
		{
			Position = AxisPosition.Left,
			Minimum = settings.IntervalY.Start,
			Maximum = settings.IntervalY.End,
			MajorStep = 1,
			MinorStep = 1,
			TickStyle = TickStyle.Inside,
		};

		plot.Axes.Add(yAxis);
		plot.Axes.Add(xAxis);

		LineSeries valid = new()
		{
			//Title = "Curve",
			Color = OxyColors.Blue,
		};

		LineSeries rejects = new()
		{
			Title = "Reject",
			Color = OxyColors.Red,
			LineStyle = LineStyle.Dash,
		};

		foreach ((IEnumerable<DataPoint> Points, bool IsValid) result in GenerateCurvesClassic(settings, lines))
		{
			if (result.IsValid)
			{
				valid.Points.AddRange(result.Points);
				valid.Points.Add(DataPoint.Undefined);
			}
			else
			{
				rejects.Points.AddRange(result.Points);
				rejects.Points.Add(DataPoint.Undefined);
			}
		}

		if (rejects.Points.Any())
		{
			plot.Series.Add(rejects);
		}

		plot.Series.Add(valid);

		plot.ResetAllAxes();

		Model = TestModel.WithLegendRightTopInside(plot);
	}

	private IEnumerable<(IEnumerable<DataPoint> Points, bool IsValid)> GenerateCurvesClassic(SurgeBinderSettings settings, int lines)
	{
		SurgeBinder generator = new SurgeBinder(settings);

		List<Curve2> rejects = new List<Curve2>();

		generator.RejectedCurve += c => rejects.Add(c);

		List<Curve2> curves = generator.Generate(lines);

		for (int scan = 0; scan < curves.Count; scan++)
		{
			yield return (CurveToPoints(curves[scan]), true);
		}

		for (int scan = 0; scan < rejects.Count; scan++)
		{
			yield return (CurveToPoints(rejects[scan]), false);
		}
	}

	private IEnumerable<DataPoint> CurveToPoints(Curve2 curve)
	{
		List<DataPoint> points = new();

		for (int scan = 0; scan < curve.Samples.Count; ++scan)
		{
			Vim.Math3d.DVector2 sample = curve.Samples[scan];
			points.Add(new DataPoint(sample.X, sample.Y));
		}

		return points;
	}

	private LineSeries CurveToSeries(Curve2 curve, string title)
	{
		LineSeries series = new()
		{
			Title = title,
		};

		series.Points.AddRange(CurveToPoints(curve));

		return series;
	}

	private void AddToGeometry(PathGeometry geometry, Curve2 curve)
	{
		IEnumerable<SWPoint> points = curve.Samples.Select(p => new SWPoint(p.X, p.Y));

		//var segment = new PolyBezierSegment(points, isStroked: true);
		var segment = new PolyLineSegment(points, isStroked: true);

		PathFigure pf = new()
		{
			StartPoint = points.First()
		};

		string dbgX = curve.Samples.SelectWithPrev((a, b, idx) =>
		{
			double val = a.X - b.X;
			return (val != 0 || idx < 2)
				? string.Empty
				: $"{idx}: {val} ({a.X})";
		})
			.Where(v => !string.IsNullOrEmpty(v))
			.ToList()
			.Pipe(l => string.Join(", ", l));

		string dbgY = curve.Samples.SelectWithPrev((a, b, idx) =>
		{
			double val = a.Y - b.Y;
			return (val != 0 || idx < 2)
				? string.Empty
				: $"{idx}: {val} ({a.Y})";
		})
			.Where(v => !string.IsNullOrEmpty(v))
			.ToList()
			.Pipe(l => string.Join(", ", l));

		pf.Segments.Add(segment);

		geometry.Figures.Add(pf);
	}
}
