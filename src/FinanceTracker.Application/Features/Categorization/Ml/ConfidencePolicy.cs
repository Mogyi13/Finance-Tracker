using System.Runtime;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>
/// UC-19: a megbízhatósági érték kiértékelése. Magas megbízhatóságnál automatikus hozzárendelés,
/// közepes és alacsony megbízhatóságnál javaslat kézi megerősítéssel; a nagyon alacsony értéket nem javasoljuk.
/// A küszöbök konfigurálhatók (<see cref="MlSettings"/>).
/// </summary>
public static class ConfidencePolicy
{
    public static ConfidenceLevel LevelOf(double confidence, MlSettings settings)
    {
        if (confidence >= settings.AutoAssignThreshold)
        {
            return ConfidenceLevel.High;
        }

        return confidence >= settings.MediumThreshold ? ConfidenceLevel.Medium : ConfidenceLevel.Low;
    }

    public static CategorizationDecision Decide(double confidence, MlSettings settings)
    {
        if (double.IsNaN(confidence))
        {
            return CategorizationDecision.None;
        }

        if (confidence >= settings.AutoAssignThreshold)
        {
            return CategorizationDecision.AutoAssign;
        }

        return confidence >= settings.MinSuggestionConfidence ? CategorizationDecision.Suggest : CategorizationDecision.None;
    }
}
