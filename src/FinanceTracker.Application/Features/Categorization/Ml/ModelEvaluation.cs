using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>A teszthalmaz egy eleme: a modell bemenete és a helyes kategória.</summary>
public sealed record EvaluationExample(string Features, Guid CategoryId);

public sealed record EvaluationRun(
    IReadOnlyList<(Guid Actual, Guid Predicted, double Confidence)> Results,
    double AveragePredictionMs);

public sealed record CriterionResult(string Name, bool Passed, string Required, string Actual, string Message);

/// <summary>A modellverzióhoz mentett teljes kiértékelési jelentés (személyes adatot nem tartalmaz).</summary>
public sealed record ModelEvaluationReport(
    string Version,
    DateTime CreatedAt,
    int LabelledCount,
    int ClassCount,
    int TrainingCount,
    int TestCount,
    double TestFraction,
    int Seed,
    ClassificationReport Model,
    double AveragePredictionMs,
    double RuleBaselineAccuracy,
    double RuleBaselineCoverage,
    double MajorityClassAccuracy,
    string? ActiveModelVersion,
    double? ActiveModelAccuracy,
    double TestDescriptionOverlap,
    IReadOnlyList<ThresholdPoint> Thresholds,
    IReadOnlyList<CriterionResult> Criteria,
    bool MeetsCriteria,
    MlSettings Settings)
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        // A magyar ékezetes szövegek olvashatóan (nem \uXXXX formában) tárolódnak.
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);
}

/// <summary>UC-20: egy modell kiértékelése a teszthalmazon (a modellfuttatást a hívó adja meg függvényként, így tesztelhető).</summary>
public static class ModelEvaluator
{
    /// <summary>
    /// Lefuttatja a predikciót minden teszt példán, és méri az átlagos feldolgozási időt. Az első hívás (a modell betöltése)
    /// kimarad az időmérésből, mert a betöltés egyszeri költség, nem predikciónként jelentkezik.
    /// </summary>
    public static EvaluationRun Evaluate(IReadOnlyList<EvaluationExample> test, Func<string, MlPrediction> predict)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(predict);

        var results = new List<(Guid, Guid, double)>(test.Count);
        if (test.Count == 0)
        {
            return new EvaluationRun(results, 0);
        }

        predict(test[0].Features);

        long ticks = 0;
        foreach (var example in test)
        {
            var start = Stopwatch.GetTimestamp();
            var prediction = predict(example.Features);
            ticks += Stopwatch.GetTimestamp() - start;
            results.Add((example.CategoryId, prediction.CategoryId, prediction.Confidence));
        }

        var averageMs = TimeSpan.FromTicks((long)(ticks * (TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency))).TotalMilliseconds / test.Count;
        return new EvaluationRun(results, averageMs);
    }

    /// <summary>A tesztpéldák mekkora hányadának azonos a normalizált szövege egy tanítópéldáéval (az ismétlődő kereskedők miatti optimizmus mérőszáma).</summary>
    public static double DescriptionOverlap(IEnumerable<string> trainFeatures, IReadOnlyList<string> testFeatures)
    {
        if (testFeatures.Count == 0)
        {
            return 0;
        }

        var known = new HashSet<string>(trainFeatures, StringComparer.Ordinal);
        return (double)testFeatures.Count(known.Contains) / testFeatures.Count;
    }
}
