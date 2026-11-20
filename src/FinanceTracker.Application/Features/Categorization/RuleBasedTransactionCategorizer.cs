using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization;

/// <summary>
/// UC-15: szabályalapú automatikus kategorizálás. A felhasználó aktív szabályait prioritás szerint értékeli ki,
/// és az első egyező szabály kategóriáját javasolja <see cref="CategorizationStatus.RuleBased"/> állapottal.
/// A szabályok és a kategóriák felhasználónként egyszer töltődnek be (a kérés élettartamára), így egy nagy
/// import sem generál soronként adatbázis-lekérdezést. Ha nincs egyező szabály, null-t ad (a tranzakció
/// kategorizálatlan marad, vagy később a gépi tanulásos modell dönthet).
/// </summary>
public sealed class RuleBasedTransactionCategorizer(
    ICategorizationRuleRepository rules,
    ICategoryRepository categories) : ITransactionCategorizer
{
    private readonly Dictionary<Guid, UserRuleSet> _cache = new();

    public async Task<CategorySuggestion?> SuggestAsync(Guid userId, TransactionType type, string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var set = await LoadAsync(userId);
        if (set.Rules.Count == 0)
        {
            return null;
        }

        var match = RuleEvaluator.FindMatch(set.Rules, set.Categories, type, description);
        return match is null ? null : new CategorySuggestion(match.CategoryId, CategorizationStatus.RuleBased);
    }

    private async Task<UserRuleSet> LoadAsync(Guid userId)
    {
        if (_cache.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        var userRules = (await rules.GetAllAsync(userId)).Where(r => r.IsActive).ToList();
        var byId = userRules.Count == 0
            ? new Dictionary<Guid, Category>()
            : (await categories.GetAvailableAsync(userId)).ToDictionary(c => c.Id);

        var set = new UserRuleSet(userRules, byId);
        _cache[userId] = set;
        return set;
    }

    private sealed record UserRuleSet(IReadOnlyList<CategorizationRule> Rules, IReadOnlyDictionary<Guid, Category> Categories);
}
