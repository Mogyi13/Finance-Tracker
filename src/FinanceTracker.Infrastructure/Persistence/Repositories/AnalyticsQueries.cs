using FinanceTracker.Application.Features.Analytics;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

/// <summary>
/// UC-21..UC-24 olvasási oldali lekérdezései. Minden lekérdezés a felhasználó azonosítójával kezdődik (BR-14),
/// az összegzés (SUM/COUNT) az adatbázisban történik, így nagy adatmennyiségnél sem a memóriában számolunk.
/// </summary>
public sealed class AnalyticsQueries(ApplicationDbContext db) : IAnalyticsQueries
{
    /// <summary>A felismeréshez betöltött kiadások felső korlátja (a legutóbbiak), hogy egy kérés memóriahasználata korlátos maradjon.</summary>
    private const int MaxExpensesForRecurrence = 100_000;

    public async Task<IReadOnlyList<TypeTotal>> GetTotalsAsync(Guid userId, DateOnly from, DateOnly to)
    {
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => new { t.Type, t.Currency })
            .Select(g => new { g.Key.Type, g.Key.Currency, Amount = g.Sum(t => t.Amount), Count = g.Count() })
            .ToListAsync();

        return rows.Select(r => new TypeTotal(r.Type, r.Currency, r.Amount, r.Count)).ToList();
    }

    public async Task<IReadOnlyList<CategoryTotal>> GetExpenseTotalsByCategoryAsync(Guid userId, DateOnly from, DateOnly to)
    {
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId
                        && t.Type == TransactionType.Expense
                        && t.TransactionDate >= from
                        && t.TransactionDate <= to)
            .GroupBy(t => new { t.CategoryId, t.Currency })
            .Select(g => new { g.Key.CategoryId, g.Key.Currency, Amount = g.Sum(t => t.Amount), Count = g.Count() })
            .ToListAsync();

        // A kategórianevek külön, egyetlen lekérdezéssel jönnek (a kategóriák száma kicsi).
        var ids = rows.Where(r => r.CategoryId != null).Select(r => r.CategoryId!.Value).Distinct().ToList();
        var names = ids.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Categories.AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

        return rows
            .Select(r => new CategoryTotal(
                r.CategoryId,
                r.CategoryId is { } id && names.TryGetValue(id, out var name) ? name : null,
                r.Currency,
                r.Amount,
                r.Count))
            .ToList();
    }

    public async Task<IReadOnlyList<ExpenseEntry>> GetExpensesSinceAsync(Guid userId, DateOnly from)
    {
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.TransactionDate >= from)
            .OrderByDescending(t => t.TransactionDate)
            .Take(MaxExpensesForRecurrence)
            .Select(t => new { t.TransactionDate, t.Amount, t.Currency, t.Description, t.CategoryId })
            .ToListAsync();

        return rows
            .Select(r => new ExpenseEntry(r.TransactionDate, r.Amount, r.Currency, r.Description, r.CategoryId))
            .ToList();
    }

    public async Task<IReadOnlyList<RecurringRow>> GetRecurringAsync(Guid userId)
    {
        var rows = await (
            from r in db.RecurringTransactions.AsNoTracking()
            where r.UserId == userId
            join c in db.Categories on r.CategoryId equals (Guid?)c.Id into categories
            from c in categories.DefaultIfEmpty()
            select new
            {
                r.Id,
                r.Description,
                r.CategoryId,
                CategoryName = c != null ? c.Name : null,
                r.ExpectedAmount,
                r.Currency,
                r.Frequency,
                r.LastOccurrence,
                r.Confidence,
                r.CreatedAt
            }).ToListAsync();

        return rows
            .Select(r => new RecurringRow(
                r.Id, r.Description, r.CategoryId, r.CategoryName, r.ExpectedAmount, r.Currency,
                r.Frequency, r.LastOccurrence, r.Confidence, r.CreatedAt))
            .ToList();
    }

    public async Task<DateTime?> GetLastTransactionChangeAsync(Guid userId)
        => await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId)
            .MaxAsync(t => (DateTime?)(t.UpdatedAt ?? t.CreatedAt));
}
