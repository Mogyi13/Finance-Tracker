using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Tranzakciókat csoportosító kategória. A rendszerkategóriák UserId értéke NULL.
/// </summary>
public class Category
{
    public const int MaxNameLength = 100;

    public Guid Id { get; set; }

    /// <summary>A tulajdonos felhasználó. NULL esetén rendszerkategória.</summary>
    public Guid? UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public bool IsSystemCategory { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Új, a felhasználó saját kategóriája. A név a megadott formában (levágva) kerül eltárolásra.</summary>
    public static Category CreateCustom(Guid id, Guid userId, string name, CategoryType type, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A kategória azonosítója nem lehet üres.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("A saját kategóriának tulajdonos felhasználó kell.");
        }

        EnsureValid(name, type);

        return new Category
        {
            Id = id,
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            IsSystemCategory = false,
            CreatedAt = createdAtUtc
        };
    }

    /// <summary>
    /// A saját kategória nevének és típusának módosítása. A rendszerkategória nem módosítható;
    /// azt, hogy a típus módosítható-e (nincs rá hivatkozó tranzakció), az Application réteg ellenőrzi.
    /// </summary>
    public void Update(string name, CategoryType type)
    {
        if (IsSystemCategory)
        {
            throw new DomainException("A rendszerkategória nem módosítható.");
        }

        EnsureValid(name, type);

        Name = name.Trim();
        Type = type;
    }

    public static bool IsValidName(string? name)
        => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxNameLength;

    private static void EnsureValid(string name, CategoryType type)
    {
        if (!IsValidName(name))
        {
            throw new DomainException($"A kategória neve kötelező, és legfeljebb {MaxNameLength} karakter lehet.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("A kategória típusa érvénytelen.");
        }
    }

    /// <summary>Rendszerkategória vagy a megadott felhasználó saját kategóriája.</summary>
    public bool IsAvailableTo(Guid userId) => UserId is null || UserId == userId;

    /// <summary>Bevétel csak bevételi, kiadás csak kiadási kategóriába sorolható.</summary>
    public bool IsCompatibleWith(TransactionType type) => (byte)Type == (byte)type;
}
