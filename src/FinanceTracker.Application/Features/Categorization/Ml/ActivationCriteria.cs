using System.Globalization;
using System.Runtime;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>A kiértékelés számai, amelyekből az aktiválási feltételek ellenőrzése számolódik.</summary>
public sealed record ActivationInputs(
    int TestCount,
    double Accuracy,
    double MacroF1,
    double AveragePredictionMs,
    double MajorityClassAccuracy,
    double RuleBaselineAccuracy,
    double? ActiveModelAccuracy);

/// <summary>
/// UC-20: az előre meghatározott aktiválási feltételek. Új modell csak akkor aktiválható, ha mindegyik teljesül.
/// A küszöbértékek konfigurációból jönnek (<see cref="MlSettings"/>), a feltételek listája rögzített.
/// </summary>
public static class ActivationCriteria
{
    public static IReadOnlyList<CriterionResult> Evaluate(ActivationInputs input, MlSettings settings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(settings);

        var results = new List<CriterionResult>
        {
            AtLeast("Tesztadatok száma", input.TestCount, settings.MinTestExamples, "{0:0}"),
            AtLeast("Pontosság (accuracy)", input.Accuracy, settings.MinAccuracy, "{0:P1}"),
            AtLeast("Makro F1", input.MacroF1, settings.MinMacroF1, "{0:P1}"),
            AtMost("Átlagos predikciós idő (ms)", input.AveragePredictionMs, settings.MaxAveragePredictionMs, "{0:0.###}"),
            Compare("Jobb a legtöbbször előforduló kategória tippelésénél", input.Accuracy, input.MajorityClassAccuracy, strict: true),
            Compare("Nem rosszabb a szabályalapú alapmegoldásnál", input.Accuracy, input.RuleBaselineAccuracy, strict: false)
        };

        if (input.ActiveModelAccuracy is { } active)
        {
            results.Add(Compare(
                "Nem rosszabb az aktív modellnél",
                input.Accuracy,
                active - settings.MaxRegressionVsActive,
                strict: false));
        }

        return results;
    }

    public static bool AllPassed(IEnumerable<CriterionResult> results) => results.All(r => r.Passed);

    private static CriterionResult AtLeast(string name, double actual, double required, string format)
    {
        var passed = actual >= required;
        return new CriterionResult(
            name,
            passed,
            "≥ " + string.Format(CultureInfo.InvariantCulture, format, required),
            string.Format(CultureInfo.InvariantCulture, format, actual),
            passed ? "Megfelel." : "Nem éri el az előírt értéket.");
    }

    private static CriterionResult AtMost(string name, double actual, double required, string format)
    {
        var passed = actual <= required;
        return new CriterionResult(
            name,
            passed,
            "≤ " + string.Format(CultureInfo.InvariantCulture, format, required),
            string.Format(CultureInfo.InvariantCulture, format, actual),
            passed ? "Megfelel." : "Meghaladja a megengedett értéket.");
    }

    private static CriterionResult Compare(string name, double actual, double reference, bool strict)
    {
        var passed = strict ? actual > reference : actual >= reference;
        return new CriterionResult(
            name,
            passed,
            (strict ? "> " : "≥ ") + reference.ToString("P1", CultureInfo.InvariantCulture),
            actual.ToString("P1", CultureInfo.InvariantCulture),
            passed ? "Megfelel." : "Nem teljesül az összehasonlítás.");
    }
}
