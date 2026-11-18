using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Infrastructure.Categorization;

/// <summary>
/// Ideiglenes automatikus kategorizáló: nem javasol kategóriát, így az automatikusan rögzített
/// tranzakciók kategorizálatlanok maradnak. A szabályalapú motor és az ML-modell (SF-024, SF-028)
/// ennek az interfésznek új implementációjaként kerül be.
/// </summary>
public sealed class NoOpTransactionCategorizer : ITransactionCategorizer
{
    public Task<CategorySuggestion?> SuggestAsync(Guid userId, TransactionType type, string? description)
        => Task.FromResult<CategorySuggestion?>(null);
}
