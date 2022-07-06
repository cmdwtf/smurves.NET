using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace cmdwtf.Smurves.Example;

internal class TestModel : IModel
{
	/// <summary>
	/// Gets the plot model.
	/// </summary>
	public PlotModel Model { get; private set; }

	public TestModel()
	{
		// Create the plot model
		// http://en.wikipedia.org/wiki/Normal_distribution

		var plot = new PlotModel
		{
			Title = "Normal distribution",
			Subtitle = "Probability density function"
		};

		plot.Axes.Add(new LinearAxis
		{
			Position = AxisPosition.Left,
			Minimum = -0.05,
			Maximum = 1.05,
			MajorStep = 0.2,
			MinorStep = 0.05,
			TickStyle = TickStyle.Inside
		});
		plot.Axes.Add(new LinearAxis
		{
			Position = AxisPosition.Bottom,
			Minimum = -5.25,
			Maximum = 5.25,
			MajorStep = 1,
			MinorStep = 0.25,
			TickStyle = TickStyle.Inside
		});

		// Add the series to the plot model
		plot.Series.Add(CreateNormalDistributionSeries(-5, 5, 0, 0.2));
		plot.Series.Add(CreateNormalDistributionSeries(-5, 5, 0, 1));
		plot.Series.Add(CreateNormalDistributionSeries(-5, 5, 0, 5));
		plot.Series.Add(CreateNormalDistributionSeries(-5, 5, -2, 0.5));

		// Axes are created automatically if they are not defined

		// Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
		Model = WithLegendRightTopInside(plot);
	}

	public static DataPointSeries CreateNormalDistributionSeries(double x0, double x1, double mean, double variance, int n = 1001)
	{
		var ls = new LineSeries
		{
			Title = string.Format("μ={0}, σ²={1}", mean, variance),
			IsVisible = true,
		};

		for (int i = 0; i < n; i++)
		{
			double x = x0 + ((x1 - x0) * i / (n - 1));
			double f = 1.0 / Math.Sqrt(2 * Math.PI * variance) * Math.Exp(-(x - mean) * (x - mean) / 2 / variance);
			ls.Points.Add(new DataPoint(x, f));
		}

		return ls;
	}

	public static PlotModel WithLegendRightTopInside(PlotModel model)
	{
		var l = new Legend
		{
			LegendPlacement = LegendPlacement.Inside,
			LegendPosition = LegendPosition.RightTop,
			LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
			LegendBorder = OxyColors.Black,
		};

		model.Legends.Add(l);

		return model;
	}
}
