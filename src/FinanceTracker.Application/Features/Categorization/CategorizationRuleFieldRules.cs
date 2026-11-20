using FinanceTracker.Application.Common;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization;

/// <summary>A szabály létrehozásának és módosításának közös validációs szabályai.</summary>
internal static class CategorizationRuleFieldRules
{
    public static void Validate(string normalizedKeyword, RuleMatchType matchType, int priority, FieldErrors errors)
    {
        if (normalizedKeyword.Length == 0)
        {
            errors.Add("keyword", "A kulcsszó megadása kötelező.");
        }
        else if (normalizedKeyword.Length > CategorizationRule.MaxKeywordLength)
        {
            errors.Add("keyword", $"A kulcsszó legfeljebb {CategorizationRule.MaxKeywordLength} karakter lehet.");
        }

        if (!Enum.IsDefined(matchType))
        {
            errors.Add("matchType", "Az egyezési típus érvénytelen.");
        }

        ValidatePriority(priority, errors);
    }

    public static void ValidatePriority(int priority, FieldErrors errors)
    {
        if (!CategorizationRule.IsValidPriority(priority))
        {
            errors.Add("priority", $"A prioritás {CategorizationRule.MinPriority} és {CategorizationRule.MaxPriority} között lehet.");
        }
    }

    /// <summary>A célkategóriának léteznie kell, és elérhetőnek kell lennie a felhasználó számára.</summary>
    public static void ValidateCategory(Category? category, Guid userId, FieldErrors errors)
    {
        if (category is null || !category.IsAvailableTo(userId))
        {
            errors.Add("categoryId", "A kategória nem található.");
        }
    }
}
