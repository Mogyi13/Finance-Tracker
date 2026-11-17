using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Transactions;

public sealed record CategorySuggestion(Guid CategoryId, CategorizationStatus Status);

/// <summary>
/// Automatikus kategorizálás bővítési pontja (szabályalapú motor, ML). Ha nem tud kategóriát
/// meghatározni, null-t ad, és a tranzakció kategorizálatlan marad.
/// </summary>
public interface ITransactionCategorizer
{
    Task<CategorySuggestion?> SuggestAsync(Guid userId, TransactionType type, string? description);
}
