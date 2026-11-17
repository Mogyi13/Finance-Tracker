using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.ValueObjects;

/// <summary>A tranzakció módosítható üzleti adatai, érvényes állapotban.</summary>
public sealed class TransactionDetails
{
    /// <summary>A decimal(18,2) oszlop által tárolható legnagyobb érték.</summary>
    public const decimal MaxAmount = 9_999_999_999_999_999.99m;

    public const int MaxDescriptionLength = 500;

    private TransactionDetails(TransactionType type, decimal amount, string currency, string? description, DateOnly date)
    {
        Type = type;
        Amount = amount;
        Currency = currency;
        Description = description;
        Date = date;
    }

    public TransactionType Type { get; }

    public decimal Amount { get; }

    public string Currency { get; }

    public string? Description { get; }

    public DateOnly Date { get; }

    public static TransactionDetails Create(
        TransactionType type,
        decimal amount,
        string currency,
        string? description,
        DateOnly date)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException("A tranzakció típusa érvénytelen.");
        }

        if (!IsValidAmount(amount))
        {
            throw new DomainException(
                "Az összegnek pozitívnak kell lennie, és legfeljebb két tizedesjegyet tartalmazhat.");
        }

        if (!SupportedCurrencies.IsSupported(currency))
        {
            throw new DomainException("A pénznem nem támogatott.");
        }

        if (description is not null && description.Length > MaxDescriptionLength)
        {
            throw new DomainException($"A leírás legfeljebb {MaxDescriptionLength} karakter lehet.");
        }

        if (date == default)
        {
            throw new DomainException("A tranzakció dátuma kötelező.");
        }

        return new TransactionDetails(
            type,
            amount,
            currency,
            string.IsNullOrWhiteSpace(description) ? null : description,
            date);
    }

    public static bool IsValidAmount(decimal amount)
        => amount > 0 && amount <= MaxAmount && decimal.Round(amount, 2) == amount;
}
