using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Bevétel vagy kiadás. Az invariánsokat (pozitív összeg, a kategória típusa és tulajdonosa) maga az entitás védi.
/// </summary>
public sealed class Transaction
{
    // Az EF Core számára.
    private Transaction()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? CategoryId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public TransactionType Type { get; private set; }

    public string? Description { get; private set; }

    public DateOnly TransactionDate { get; private set; }

    public TransactionSource Source { get; private set; }

    public CategorizationStatus CategorizationStatus { get; private set; }

    /// <summary>Létrehozás időpontja (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Utolsó módosítás időpontja (UTC).</summary>
    public DateTime? UpdatedAt { get; private set; }

    public static Transaction Create(
        Guid id,
        Guid userId,
        TransactionDetails details,
        TransactionSource source,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A tranzakció azonosítója nem lehet üres.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("A tranzakciónak tulajdonos felhasználó kell.");
        }

        if (!Enum.IsDefined(source))
        {
            throw new DomainException("A tranzakció forrása érvénytelen.");
        }

        var transaction = new Transaction
        {
            Id = id,
            UserId = userId,
            Source = source,
            CreatedAt = createdAtUtc,
            CategorizationStatus = CategorizationStatus.Uncategorized
        };
        transaction.ApplyDetails(details);
        return transaction;
    }

    /// <summary>Kategória hozzárendelése (kézi vagy automatikus kategorizálás eredményeként).</summary>
    public void AssignCategory(Category category, CategorizationStatus status)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (status == CategorizationStatus.Uncategorized || !Enum.IsDefined(status))
        {
            throw new DomainException("Kategória hozzárendeléséhez érvényes kategorizálási állapot kell.");
        }

        EnsureCategoryUsable(category, Type);
        CategoryId = category.Id;
        CategorizationStatus = status;
    }

    public void ClearCategory()
    {
        CategoryId = null;
        CategorizationStatus = CategorizationStatus.Uncategorized;
    }

    /// <summary>
    /// A tranzakció teljes módosítása. A kategória nélkül (null) a tranzakció kategorizálatlan lesz;
    /// változatlan kategória esetén a korábbi kategorizálási állapot megmarad.
    /// </summary>
    public void Update(TransactionDetails details, Category? category, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(details);

        // Előbb minden ellenőrzés, csak utána módosítás, hogy hiba esetén az entitás ne maradjon félig módosítva.
        if (category is not null)
        {
            EnsureCategoryUsable(category, details.Type);
        }

        var previousCategoryId = CategoryId;
        ApplyDetails(details);

        if (category is null)
        {
            ClearCategory();
        }
        else if (category.Id != previousCategoryId)
        {
            CategoryId = category.Id;
            CategorizationStatus = CategorizationStatus.ManuallyAssigned;
        }

        UpdatedAt = updatedAtUtc;
    }

    private void ApplyDetails(TransactionDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        Type = details.Type;
        Amount = details.Amount;
        Currency = details.Currency;
        Description = details.Description;
        TransactionDate = details.Date;
    }

    private void EnsureCategoryUsable(Category category, TransactionType type)
    {
        if (!category.IsAvailableTo(UserId))
        {
            throw new DomainException("A kategória nem érhető el a felhasználó számára.");
        }

        if (!category.IsCompatibleWith(type))
        {
            throw new DomainException("A kategória típusa nem egyezik a tranzakció típusával.");
        }
    }
}
