namespace FinanceTracker.Application.Features.Forecasting;

/// <summary>
/// UC-28: két egyszerű, magyarázható előrejelző módszer havi idősorra (tiszta logika).
/// A mozgóátlag a legutóbbi N hónap átlaga; a további hónapokra a korábbi becsléseit is felhasználja (ezért simul).
/// A lineáris regresszió a legkisebb négyzetek módszerével illesztett egyenes folytatása.
/// </summary>
public static class ForecastCalculator
{
    public static IReadOnlyList<decimal> MovingAverage(IReadOnlyList<decimal> values, int window, int horizon)
    {
        var working = values.ToList();
        var result = new List<decimal>(horizon);
        for (var i = 0; i < horizon; i++)
        {
            var take = Math.Min(window, working.Count);
            var next = Math.Round(working.Skip(working.Count - take).Average(), 2);
            working.Add(next);
            result.Add(next);
        }

        return result;
    }

    /// <summary>Lineáris regresszió; <paramref name="nonNegative"/> esetén a negatív becslés 0-ra vágódik (a kiadás nem lehet negatív).</summary>
    public static IReadOnlyList<decimal> LinearRegression(IReadOnlyList<decimal> values, int horizon, bool nonNegative)
    {
        var n = values.Count;
        var (slope, intercept) = Fit(values);

        var result = new List<decimal>(horizon);
        for (var h = 0; h < horizon; h++)
        {
            var predicted = Math.Round(intercept + slope * (n + h), 2);
            result.Add(nonNegative && predicted < 0 ? 0m : predicted);
        }

        return result;
    }

    internal static (decimal Slope, decimal Intercept) Fit(IReadOnlyList<decimal> values)
    {
        var n = values.Count;
        if (n == 0)
        {
            return (0m, 0m);
        }

        if (n == 1)
        {
            return (0m, values[0]);
        }

        var meanX = (n - 1) / 2m;
        var meanY = values.Average();
        decimal numerator = 0, denominator = 0;
        for (var i = 0; i < n; i++)
        {
            numerator += (i - meanX) * (values[i] - meanY);
            denominator += (i - meanX) * (i - meanX);
        }

        var slope = denominator == 0 ? 0m : numerator / denominator;
        return (slope, meanY - slope * meanX);
    }
}
