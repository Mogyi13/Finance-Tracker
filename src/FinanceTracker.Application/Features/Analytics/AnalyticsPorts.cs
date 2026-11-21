using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Analytics;

/// <summary>Egy pénznem és tranzakciótípus összesítése egy időszakra (az adatbázisban csoportosítva).</summary>
public sealed record TypeTotal(TransactionType Type, string Currency, decimal Amount, int Count);

/// <summary>Egy kategória és pénznem kiadás-összesítése egy időszakra; a null kategória a kategorizálatlan tételek.</summary>
public sealed record CategoryTotal(Guid? CategoryId, string? CategoryName, string Currency, decimal Amount, int Count);

/// <summary>Egy kiadás a felismeréshez (csak a szükséges mezők, felhasználói azonosító nélkül).</summary>
public sealed record ExpenseEntry(DateOnly Date, decimal Amount, string Currency, string? Description, Guid? CategoryId);

/// <summary>Egy mentett ismétlődő kiadás a kategórianévvel.</summary>
public sealed record RecurringRow(
    Guid Id,
    string Description,
    Guid? CategoryId,
    string? CategoryName,
    decimal ExpectedAmount,
    string Currency,
    RecurrenceFrequency Frequency,
    DateOnly LastOccurrence,
    decimal Confidence,
    DateTime CreatedAt);

/// <summary>
/// Olvasási oldali elemzési lekérdezések. Minden metódus a megadott felhasználó adataira szűr, és az összegzést
/// az adatbázisban végzi (nem tölti be a tranzakciókat a memóriába). A dátumhatárok zártak (mindkét nap beletartozik).
/// </summary>
public interface IAnalyticsQueries
{
    Task<IReadOnlyList<TypeTotal>> GetTotalsAsync(Guid userId, DateOnly from, DateOnly to);

    Task<IReadOnlyList<CategoryTotal>> GetExpenseTotalsByCategoryAsync(Guid userId, DateOnly from, DateOnly to);

    /// <summary>A felhasználó kiadásai a megadott naptól (UC-24 bemenete).</summary>
    Task<IReadOnlyList<ExpenseEntry>> GetExpensesSinceAsync(Guid userId, DateOnly from);

    /// <summary>A felhasználó mentett ismétlődő kiadásai.</summary>
    Task<IReadOnlyList<RecurringRow>> GetRecurringAsync(Guid userId);

    /// <summary>A legutóbbi tranzakció-létrehozás vagy -módosítás ideje (UTC); null, ha nincs tranzakció.</summary>
    Task<DateTime?> GetLastTransactionChangeAsync(Guid userId);
}

public interface IRecurringTransactionRepository
{
    /// <summary>A felhasználó korábbi eredményeinek lecserélése az újakra egyetlen tranzakcióban.</summary>
    Task ReplaceAsync(Guid userId, IReadOnlyList<RecurringTransaction> items);
}
