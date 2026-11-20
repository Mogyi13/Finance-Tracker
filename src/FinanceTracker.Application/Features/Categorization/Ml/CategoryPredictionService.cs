using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>Egy tranzakcióleírás teljes előrejelzése: a felhasználó szabálya vagy az ML-modell találata, a döntéssel együtt.</summary>
public sealed record CategoryPrediction(
    PredictionSource Source,
    Guid? CategoryId,
    double? Confidence,
    ConfidenceLevel? Level,
    CategorizationDecision Decision,
    string? ModelVersion,
    MlOutcome? ModelOutcome);

/// <summary>
/// UC-18 és UC-19 olvasási oldali változata (nem módosít semmit): a predikciós és a javaslatok végpont használja.
/// Ugyanazt a sorrendet követi, mint a rögzítéskor történő automatikus kategorizálás: előbb a szabályok, utána a modell.
/// </summary>
public sealed class CategoryPredictionService(RuleBasedTransactionCategorizer rules, MlCategoryService ml)
{
    public async Task<CategoryPrediction> PredictAsync(Guid userId, TransactionType type, string? description)
    {
        var byRule = await rules.SuggestAsync(userId, type, description);
        if (byRule is not null)
        {
            return new CategoryPrediction(
                PredictionSource.Rule, byRule.CategoryId, 1.0, ConfidenceLevel.High, CategorizationDecision.AutoAssign, null, null);
        }

        var result = await ml.PredictAsync(userId, type, description);
        if (result.Outcome != MlOutcome.Predicted)
        {
            return new CategoryPrediction(PredictionSource.None, null, null, null, CategorizationDecision.None, result.ModelVersion, result.Outcome);
        }

        return new CategoryPrediction(
            result.Decision == CategorizationDecision.None ? PredictionSource.None : PredictionSource.Model,
            result.Decision == CategorizationDecision.None ? null : result.CategoryId,
            result.Confidence,
            result.Level,
            result.Decision,
            result.ModelVersion,
            result.Outcome);
    }
}
