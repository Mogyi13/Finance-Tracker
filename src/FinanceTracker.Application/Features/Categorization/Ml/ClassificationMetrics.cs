namespace FinanceTracker.Application.Features.Categorization.Ml;

public sealed record ClassMetrics(Guid CategoryId, int Support, int Predicted, double Precision, double Recall, double F1);

public sealed record ClassificationReport(
    int Count,
    double Accuracy,
    double MacroPrecision,
    double MacroRecall,
    double MacroF1,
    IReadOnlyList<ClassMetrics> Classes);

/// <summary>Egy küszöbérték hatása: a tesztadatok mekkora részére ad a modell legalább ekkora megbízhatóságú választ, és azok mennyire pontosak.</summary>
public sealed record ThresholdPoint(double Threshold, double Coverage, double? Accuracy, int Count);

/// <summary>UC-20: a többosztályos besorolás értékelő mutatói (tiszta számítás, nincs függése az ML.NET-től).</summary>
public static class ClassificationMetrics
{
    public static readonly double[] DefaultThresholds = [0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95];

    /// <summary>
    /// Pontosság (accuracy) és makro átlagolt precision / recall / F1. A makro átlag azokon az osztályokon számolódik,
    /// amelyekből van a tesztadatban (support &gt; 0), így a ritka kategóriák ugyanúgy számítanak, mint a gyakoriak.
    /// Ha egy osztályra nem született predikció, a precision 0. A nem értelmezhető predikció (<see cref="Guid.Empty"/>) hibának számít.
    /// </summary>
    public static ClassificationReport Compute(IReadOnlyList<(Guid Actual, Guid Predicted)> pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);

        if (pairs.Count == 0)
        {
            return new ClassificationReport(0, 0, 0, 0, 0, Array.Empty<ClassMetrics>());
        }

        var correct = pairs.Count(p => p.Actual == p.Predicted);
        var classes = new List<ClassMetrics>();

        foreach (var group in pairs.GroupBy(p => p.Actual).OrderBy(g => g.Key))
        {
            var category = group.Key;
            var support = group.Count();
            var truePositives = group.Count(p => p.Predicted == category);
            var predicted = pairs.Count(p => p.Predicted == category);

            var precision = predicted == 0 ? 0 : (double)truePositives / predicted;
            var recall = (double)truePositives / support;
            var f1 = precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall);
            classes.Add(new ClassMetrics(category, support, predicted, precision, recall, f1));
        }

        return new ClassificationReport(
            pairs.Count,
            (double)correct / pairs.Count,
            classes.Average(c => c.Precision),
            classes.Average(c => c.Recall),
            classes.Average(c => c.F1),
            classes);
    }

    /// <summary>A legtöbbször előforduló tanító kategória mint "buta" alapmegoldás pontossága a tesztadatokon.</summary>
    public static double MajorityClassAccuracy(IEnumerable<Guid> trainLabels, IReadOnlyList<Guid> testLabels)
    {
        if (testLabels.Count == 0)
        {
            return 0;
        }

        var majority = trainLabels
            .GroupBy(l => l)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => (Guid?)g.Key)
            .FirstOrDefault();

        return majority is null ? 0 : (double)testLabels.Count(l => l == majority) / testLabels.Count;
    }

    /// <summary>UC-19 alátámasztása: küszöbönként a lefedettség és a pontosság, hogy a végleges küszöbök a mérésből adódjanak.</summary>
    public static IReadOnlyList<ThresholdPoint> ThresholdAnalysis(
        IReadOnlyList<(Guid Actual, Guid Predicted, double Confidence)> results,
        IEnumerable<double>? thresholds = null)
    {
        ArgumentNullException.ThrowIfNull(results);

        var points = new List<ThresholdPoint>();
        foreach (var threshold in thresholds ?? DefaultThresholds)
        {
            var above = results.Where(r => r.Confidence >= threshold).ToList();
            double? accuracy = above.Count == 0 ? null : (double)above.Count(r => r.Actual == r.Predicted) / above.Count;
            var coverage = results.Count == 0 ? 0 : (double)above.Count / results.Count;
            points.Add(new ThresholdPoint(threshold, coverage, accuracy, above.Count));
        }

        return points;
    }
}
