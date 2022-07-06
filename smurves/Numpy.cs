using System;
using System.Collections.Generic;
using System.Linq;

namespace cmdwtf.Smurves;

// via https://gist.github.com/wcharczuk/3948606
internal static class Numpy
{
	public static IEnumerable<SmurveComponent> Arange(SmurveComponent start, int count)
	{
		return Enumerable.Range((int)start, count).Select(v => (SmurveComponent)v);
	}

	public static IEnumerable<SmurveComponent> Power(IEnumerable<SmurveComponent> exponents, SmurveComponent baseValue = 10.0d)
	{
		return exponents.Select(v => Math.Pow(baseValue, v));
	}

	public static IEnumerable<SmurveComponent> LinSpace(SmurveComponent start, SmurveComponent stop, int num, bool endpoint = true)
	{
		IEnumerable<SmurveComponent> result = new List<SmurveComponent>();
		if (num <= 0)
		{
			return result;
		}

		if (endpoint)
		{
			if (num == 1)
			{
				return new List<SmurveComponent>() { start };
			}

			var step = (stop - start) / ((SmurveComponent)num - 1.0d);
			result = Arange(0, num).Select(v => (v * step) + start).ToList();
		}
		else
		{
			var step = (stop - start) / (SmurveComponent)num;
			result = Arange(0, num).Select(v => (v * step) + start).ToList();
		}

		return result;
	}

	public static IEnumerable<SmurveComponent> LogSpace(SmurveComponent start, SmurveComponent stop, int num, bool endpoint = true, SmurveComponent numericBase = 10.0d)
	{
		IEnumerable<SmurveComponent> y = LinSpace(start, stop, num: num, endpoint: endpoint);
		return Power(y, numericBase);
	}
}
