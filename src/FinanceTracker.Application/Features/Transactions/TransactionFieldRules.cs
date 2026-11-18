using FinanceTracker.Application.Common;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Transactions;

/// <summary>A tranzakció létrehozásának és módosításának közös validációs szabályai.</summary>
internal static class TransactionFieldRules
{
    public static void Validate(
        TransactionType type,
        decimal amount,
        string currency,
        string? description,
        DateOnly date,
        FieldErrors errors)
    {
        if (!Enum.IsDefined(type))
        {
            errors.Add("type", "A tranzakció típusa érvénytelen.");
        }

        if (amount <= 0)
        {
            errors.Add("amount", "Az összegnek pozitívnak kell lennie.");
        }
        else if (amount > TransactionDetails.MaxAmount)
        {
            errors.Add("amount", "Az összeg túl nagy.");
        }
        else if (decimal.Round(amount, 2) != amount)
        {
            errors.Add("amount", "Az összeg legfeljebb két tizedesjegyet tartalmazhat.");
        }

        if (!SupportedCurrencies.IsSupported(currency))
        {
            errors.Add("currency", $"A pénznem nem támogatott. Támogatott pénznemek: {string.Join(", ", SupportedCurrencies.All)}.");
        }

        if (description is not null && description.Length > TransactionDetails.MaxDescriptionLength)
        {
            errors.Add("description", $"A leírás legfeljebb {TransactionDetails.MaxDescriptionLength} karakter lehet.");
        }

        if (date == default)
        {
            errors.Add("transactionDate", "A tranzakció dátuma kötelező.");
        }
    }

    /// <summary>
    /// A kiválasztott kategória ellenőrzése. A más felhasználóhoz tartozó kategóriáról nem derül ki,
    /// hogy létezik, ugyanazt az üzenetet kapja, mint a nem létező.
    /// </summary>
    public static void ValidateCategory(Category? category, Guid userId, TransactionType type, FieldErrors errors)
    {
        if (category is null || !category.IsAvailableTo(userId))
        {
            errors.Add("categoryId", "A kategória nem létezik vagy nem érhető el.");
        }
        else if (Enum.IsDefined(type) && !category.IsCompatibleWith(type))
        {
            errors.Add("categoryId", "A kategória típusa nem egyezik a tranzakció típusával.");
        }
    }

    public static string NormalizeCurrency(string? currency) => (currency ?? string.Empty).Trim().ToUpperInvariant();

    public static string? NormalizeDescription(string? description)
    {
        var trimmed = description?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
