using FinanceTracker.Application.Features.Categorization.Ml;

namespace FinanceTracker.Application.Features.Categorization;

/// <summary>
/// Egy előrejelzés az API-n át: honnan jött (szabály vagy modell), melyik kategóriát javasolja, milyen megbízhatósággal,
/// és mit javasol a rendszer (automatikus hozzárendelés, megerősítést kérő javaslat vagy semmi).
/// </summary>
public sealed record CategoryPredictionDto(
    PredictionSource Source,
    Guid? CategoryId,
    string? CategoryName,
    double? Confidence,
    ConfidenceLevel? Level,
    CategorizationDecision Decision,
    string? ModelVersion,
    string Message)
{
    internal static CategoryPredictionDto From(CategoryPrediction prediction, string? categoryName)
        => new(
            prediction.Source,
            prediction.CategoryId,
            categoryName,
            prediction.Confidence is { } c ? Math.Round(c, 4) : null,
            prediction.Level,
            prediction.Decision,
            prediction.ModelVersion,
            MessageFor(prediction));

    private static string MessageFor(CategoryPrediction p) => (p.Source, p.Decision, p.ModelOutcome) switch
    {
        (PredictionSource.Rule, _, _) => "A kategória a saját szabályaid alapján adódik.",
        (PredictionSource.Model, CategorizationDecision.AutoAssign, _) => "Magas megbízhatóságú modellpredikció: automatikusan hozzárendelhető.",
        (PredictionSource.Model, CategorizationDecision.Suggest, _) => "A modell javaslata: kérjük, erősítsd meg vagy javítsd.",
        (_, _, MlOutcome.NoModel) => "Nincs aktív modell, a tranzakció kategorizálatlan marad.",
        (_, _, MlOutcome.NoText) => "A leírásból nem képezhető modellbemenet.",
        (_, _, MlOutcome.Rejected) => "A modell válasza érvénytelen kategória volt, ezért elvetettük.",
        _ => "Nincs elég megbízható javaslat."
    };
}
