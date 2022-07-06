using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;

namespace cmdwtf.Smurves.Example;

public interface IModel
{
	/// <summary>
	/// Gets the plot model.
	/// </summary>
	PlotModel Model { get; }
}
