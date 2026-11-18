using FinanceTracker.Application.Common;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categories;

/// <summary>A kategória létrehozásának és módosításának közös validációs szabályai.</summary>
internal static class CategoryFieldRules
{
    public static string NormalizeName(string? name) => (name ?? string.Empty).Trim();

    public static void Validate(string normalizedName, CategoryType type, FieldErrors errors)
    {
        if (normalizedName.Length == 0)
        {
            errors.Add("name", "A kategória nevének megadása kötelező.");
        }
        else if (normalizedName.Length > Category.MaxNameLength)
        {
            errors.Add("name", $"A kategória neve legfeljebb {Category.MaxNameLength} karakter lehet.");
        }

        if (!Enum.IsDefined(type))
        {
            errors.Add("type", "A kategória típusa érvénytelen.");
        }
    }
}
