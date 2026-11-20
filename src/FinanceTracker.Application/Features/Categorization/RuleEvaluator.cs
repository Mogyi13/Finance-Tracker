using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization;

/// <summary>
/// UC-15: a szabályalapú kiértékelés tiszta logikája. Az inaktív szabályokat kihagyja, a szabályokat prioritás
/// szerint csökkenően (azonos prioritásnál a régebbi előbb) értékeli ki, és az első érvényes egyezést adja vissza.
/// Egy szabály csak akkor érvényes, ha a célkategóriája létezik, elérhető a szabály tulajdonosának,
/// és a típusa megegyezik a tranzakció típusával.
/// </summary>
public static class RuleEvaluator
{
    public static CategorizationRule? FindMatch(
        IEnumerable<CategorizationRule> rules,
        IReadOnlyDictionary<Guid, Category> categories,
        TransactionType type,
        string? description)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(categories);

        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return rules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .FirstOrDefault(r =>
                categories.TryGetValue(r.CategoryId, out var category)
                && category.IsAvailableTo(r.UserId)
                && category.IsCompatibleWith(type)
                && r.Matches(description));
    }
}
