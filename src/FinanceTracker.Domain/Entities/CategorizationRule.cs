using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// A felhasználó kulcsszó alapú kategorizálási szabálya: ha a tranzakció leírása illeszkedik a kulcsszóra,
/// a szabály célkategóriája kerül a tranzakcióhoz. A magasabb prioritású szabály értékelődik ki előbb.
/// A kulcsszó összehasonlítása a leírás normalizálása után történik (kis- és nagybetűre érzéketlen,
/// a szóközök összevonásával), lásd <see cref="TransactionFingerprint.NormalizeDescription"/>.
/// </summary>
public sealed class CategorizationRule
{
    public const int MaxKeywordLength = 200;
    public const int MinPriority = 0;
    public const int MaxPriority = 1_000_000;
    public const int DefaultPriority = 100;

    // Az EF Core számára.
    private CategorizationRule()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid CategoryId { get; private set; }

    /// <summary>A kulcsszó szóközei összevonva, a megadott kis- és nagybetűkkel (megjelenítéshez).</summary>
    public string Keyword { get; private set; } = string.Empty;

    public RuleMatchType MatchType { get; private set; }

    /// <summary>Minél nagyobb, annál korábban értékelődik ki.</summary>
    public int Priority { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static CategorizationRule Create(
        Guid id,
        Guid userId,
        Category category,
        string keyword,
        RuleMatchType matchType,
        int priority,
        bool isActive,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A szabály azonosítója nem lehet üres.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("A szabálynak tulajdonos felhasználó kell.");
        }

        var rule = new CategorizationRule
        {
            Id = id,
            UserId = userId,
            CreatedAt = createdAtUtc
        };
        rule.Apply(category, keyword, matchType, priority, isActive);
        return rule;
    }

    /// <summary>A szabály teljes módosítása (kulcsszó, egyezési típus, kategória, prioritás, aktív állapot).</summary>
    public void Update(
        Category category,
        string keyword,
        RuleMatchType matchType,
        int priority,
        bool isActive,
        DateTime updatedAtUtc)
    {
        Apply(category, keyword, matchType, priority, isActive);
        UpdatedAt = updatedAtUtc;
    }

    public void SetPriority(int priority, DateTime updatedAtUtc)
    {
        EnsureValidPriority(priority);
        Priority = priority;
        UpdatedAt = updatedAtUtc;
    }

    public void SetActive(bool isActive, DateTime updatedAtUtc)
    {
        IsActive = isActive;
        UpdatedAt = updatedAtUtc;
    }

    /// <summary>A kulcsszó szóközeinek összevonása; ez a tárolt forma.</summary>
    public static string NormalizeKeyword(string? keyword)
        => string.Join(' ', (keyword ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static bool IsValidKeyword(string? keyword)
    {
        var normalized = NormalizeKeyword(keyword);
        return normalized.Length > 0 && normalized.Length <= MaxKeywordLength;
    }

    public static bool IsValidPriority(int priority) => priority is >= MinPriority and <= MaxPriority;

    /// <summary>Illeszkedik-e a szabály a tranzakció leírására. A leírás nem szükséges előre normalizálva legyen.</summary>
    public bool Matches(string? description)
    {
        var normalizedDescription = TransactionFingerprint.NormalizeDescription(description);
        if (normalizedDescription.Length == 0)
        {
            return false;
        }

        var normalizedKeyword = TransactionFingerprint.NormalizeDescription(Keyword);
        return MatchType switch
        {
            RuleMatchType.Exact => normalizedDescription.Equals(normalizedKeyword, StringComparison.Ordinal),
            RuleMatchType.Contains => normalizedDescription.Contains(normalizedKeyword, StringComparison.Ordinal),
            RuleMatchType.StartsWith => normalizedDescription.StartsWith(normalizedKeyword, StringComparison.Ordinal),
            _ => false
        };
    }

    private void Apply(Category category, string keyword, RuleMatchType matchType, int priority, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (!IsValidKeyword(keyword))
        {
            throw new DomainException($"A kulcsszó kötelező, és legfeljebb {MaxKeywordLength} karakter lehet.");
        }

        if (!Enum.IsDefined(matchType))
        {
            throw new DomainException("Az egyezési típus érvénytelen.");
        }

        EnsureValidPriority(priority);

        if (!category.IsAvailableTo(UserId))
        {
            throw new DomainException("A kategória nem érhető el a felhasználó számára.");
        }

        CategoryId = category.Id;
        Keyword = NormalizeKeyword(keyword);
        MatchType = matchType;
        Priority = priority;
        IsActive = isActive;
    }

    private static void EnsureValidPriority(int priority)
    {
        if (!IsValidPriority(priority))
        {
            throw new DomainException($"A prioritás {MinPriority} és {MaxPriority} között lehet.");
        }
    }
}
