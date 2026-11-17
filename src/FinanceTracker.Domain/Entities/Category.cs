using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Tranzakciókat csoportosító kategória. A rendszerkategóriák UserId értéke NULL.
/// </summary>
public class Category
{
    public Guid Id { get; set; }

    /// <summary>A tulajdonos felhasználó. NULL esetén rendszerkategória.</summary>
    public Guid? UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public bool IsSystemCategory { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Rendszerkategória vagy a megadott felhasználó saját kategóriája.</summary>
    public bool IsAvailableTo(Guid userId) => UserId is null || UserId == userId;

    /// <summary>Bevétel csak bevételi, kiadás csak kiadási kategóriába sorolható.</summary>
    public bool IsCompatibleWith(TransactionType type) => (byte)Type == (byte)type;
}
