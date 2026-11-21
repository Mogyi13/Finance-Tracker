using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Forecasting;

/// <summary>
/// UC-28: a két módszer összehasonlítása visszamenőleges, egy lépéses teszttel: minden hónapot a megelőző hónapokból becslünk,
/// és összevetjük a tényleges értékkel (MAE, RMSE, MAPE). A MAPE a 0 tényleges értékű hónapokat kihagyja.
/// </summary>
public static class ForecastEvaluator
{
    public const int MinTestedMonths = 2;

    public static ForecastEvaluationCurrencyDto Evaluate(string currency, IReadOnlyList<decimal> values, ForecastType type, ForecastSettings settings)
    {
        var window = settings.EffectiveWindow;
        var minTrain = settings.EffectiveMinHistory;
        var tested = values.Count - minTrain;

        if (tested < MinTestedMonths)
        {
            return new ForecastEvaluationCurrencyDto(currency, Math.Max(tested, 0), Array.Empty<MethodMetricsDto>(), null,
                $"Az értékeléshez legalább {minTrain + MinTestedMonths} lezárt hónap kell, jelenleg {values.Count} van.");
        }

        var nonNegative = type == ForecastType.MonthlyExpenses;
        var maErrors = new List<(decimal Actual, decimal Predicted)>();
        var lrErrors = new List<(decimal Actual, decimal Predicted)>();

        for (var t = minTrain; t < values.Count; t++)
        {
            var train = values.Take(t).ToList();
            maErrors.Add((values[t], ForecastCalculator.MovingAverage(train, window, 1)[0]));
            lrErrors.Add((values[t], ForecastCalculator.LinearRegression(train, 1, nonNegative)[0]));
        }

        var ma = Metrics(ForecastMethod.MovingAverage, maErrors);
        var lr = Metrics(ForecastMethod.LinearRegression, lrErrors);
        ForecastMethod? best = ma.Mae == lr.Mae ? null : ma.Mae < lr.Mae ? ForecastMethod.MovingAverage : ForecastMethod.LinearRegression;

        return new ForecastEvaluationCurrencyDto(currency, tested, new[] { ma, lr }, best, null);
    }

    private static MethodMetricsDto Metrics(ForecastMethod method, List<(decimal Actual, decimal Predicted)> pairs)
    {
        var abs = pairs.Select(p => Math.Abs(p.Actual - p.Predicted)).ToList();
        var mae = abs.Average();
        var rmse = (decimal)Math.Sqrt((double)pairs.Average(p => (p.Actual - p.Predicted) * (p.Actual - p.Predicted)));

        var nonZero = pairs.Where(p => p.Actual != 0).ToList();
        decimal? mape = nonZero.Count == 0
            ? null
            : Math.Round(nonZero.Average(p => Math.Abs(p.Actual - p.Predicted) / Math.Abs(p.Actual)), 4);

        return new MethodMetricsDto(method, Math.Round(mae, 2), Math.Round(rmse, 2), mape);
    }
}
