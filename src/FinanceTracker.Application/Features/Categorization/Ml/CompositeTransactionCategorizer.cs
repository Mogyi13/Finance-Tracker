using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>
/// Az automatikus kategorizálás teljes sorrendje a tranzakció rögzítésekor és az importnál:
/// előbb a felhasználó szabályai (UC-15, BR-06: a szabály felülír minden modellt), ha egyik sem illeszkedik,
/// akkor az ML-modell (UC-18) – de csak magas megbízhatóságnál rendel hozzá automatikusan (UC-19).
/// A közepes és alacsony megbízhatóságú találat nem kerül mentésre; az a javaslatok oldalán jelenik meg.
/// </summary>
public sealed class CompositeTransactionCategorizer(
    RuleBasedTransactionCategorizer rules,
    MlCategoryService ml) : ITransactionCategorizer
{
    public async Task<CategorySuggestion?> SuggestAsync(Guid userId, TransactionType type, string? description)
    {
        var byRule = await rules.SuggestAsync(userId, type, description);
        if (byRule is not null)
        {
            return byRule;
        }

        var result = await ml.PredictAsync(userId, type, description);
        return result is { Outcome: MlOutcome.Predicted, Decision: CategorizationDecision.AutoAssign, CategoryId: { } categoryId }
            ? new CategorySuggestion(categoryId, CategorizationStatus.MachineLearning)
            : null;
    }
}
