using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// UC-24: a rendszer által felismert, valószínűleg ismétlődő kiadás. Becslés, nem megerősített előfizetés:
/// a felismerés a korábbi tranzakciók dátumközein és összegein alapul, ezért a <see cref="Confidence"/> mindig 1-nél kisebb.
/// Származtatott adat: az elemzés újrafuttatásakor a felhasználó korábbi eredményei lecserélődnek.
/// </summary>
public sealed class RecurringTransaction
{
    public const int MaxDescriptionLength = 200;

    // Az EF Core számára.
    private RecurringTransaction()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>A felismert csoport jellemző (leggyakoribb) leírása.</summary>
    public string Description { get; private set; } = string.Empty;

    public Guid? CategoryId { get; private set; }

    /// <summary>Becsült összeg (a csoport összegeinek mediánja).</summary>
    public decimal ExpectedAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public RecurrenceFrequency Frequency { get; private set; }

    public DateOnly LastOccurrence { get; private set; }

    /// <summary>Felismerési bizonyosság, 0 és 1 között.</summary>
    public decimal Confidence { get; private set; }

    /// <summary>A felismerés időpontja (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    public static RecurringTransaction Create(
        Guid id,
        Guid userId,
        string description,
        Guid? categoryId,
        decimal expectedAmount,
        string currency,
        RecurrenceFrequency frequency,
        DateOnly lastOccurrence,
        decimal confidence,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Az ismétlődő kiadásnak azonosító és tulajdonos kell.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Az ismétlődő kiadás leírása kötelező.");
        }

        if (expectedAmount <= 0)
        {
            throw new DomainException("A becsült összegnek pozitívnak kell lennie.");
        }

        if (currency is not { Length: 3 })
        {
            throw new DomainException("A pénznem háromkarakteres kód.");
        }

        if (!Enum.IsDefined(frequency))
        {
            throw new DomainException("Az ismétlődés gyakorisága érvénytelen.");
        }

        if (confidence is < 0 or > 1)
        {
            throw new DomainException("A bizonyosság 0 és 1 között lehet.");
        }

        var trimmed = description.Trim();
        return new RecurringTransaction
        {
            Id = id,
            UserId = userId,
            Description = trimmed.Length > MaxDescriptionLength ? trimmed[..MaxDescriptionLength] : trimmed,
            CategoryId = categoryId,
            ExpectedAmount = expectedAmount,
            Currency = currency,
            Frequency = frequency,
            LastOccurrence = lastOccurrence,
            Confidence = confidence,
            CreatedAt = createdAtUtc
        };
    }
}
