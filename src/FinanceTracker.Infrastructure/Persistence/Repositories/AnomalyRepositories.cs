using FinanceTracker.Application.Features.Anomalies;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

/// <summary>UC-25 olvasási oldali lekérdezései; minden lekérdezés a felhasználó azonosítójára szűr (BR-14).</summary>
public sealed class AnomalyQueries(ApplicationDbContext db) : IAnomalyQueries
{
    /// <summary>A betöltött kiadások felső korlátja (a legutóbbiak), hogy egy kérés memóriahasználata korlátos maradjon.</summary>
    private const int MaxExpenses = 100_000;

    public async Task<IReadOnlyList<AnomalyCandidate>> GetExpensesSinceAsync(Guid userId, DateOnly from)
    {
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.CategoryId != null && t.TransactionDate >= from)
            .OrderByDescending(t => t.TransactionDate)
            .Take(MaxExpenses)
            .Select(t => new { t.Id, t.TransactionDate, t.Amount, t.Currency, t.CategoryId })
            .ToListAsync();

        var ids = rows.Select(r => r.CategoryId!.Value).Distinct().ToList();
        var names = ids.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Categories.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);

        return rows
            .Select(r => new AnomalyCandidate(r.Id, r.TransactionDate, r.Amount, r.Currency, r.CategoryId,
                names.TryGetValue(r.CategoryId!.Value, out var name) ? name : null))
            .ToList();
    }

    public async Task<IReadOnlyList<AnomalyDto>> GetAsync(Guid userId, bool includeReviewed)
    {
        var rows = await (
                from a in db.FinancialAnomalies.AsNoTracking()
                join t in db.Transactions.AsNoTracking() on a.TransactionId equals t.Id
                join c in db.Categories.AsNoTracking() on t.CategoryId equals c.Id into categories
                from c in categories.DefaultIfEmpty()
                where a.UserId == userId && (includeReviewed || !a.IsReviewed)
                orderby a.IsReviewed, a.Score descending
                select new
                {
                    a.Id,
                    a.TransactionId,
                    a.AnomalyType,
                    a.Score,
                    a.Description,
                    a.DetectedAt,
                    a.IsReviewed,
                    t.TransactionDate,
                    t.Amount,
                    t.Currency,
                    TransactionDescription = t.Description,
                    CategoryId = (Guid?)t.CategoryId,
                    CategoryName = c.Name
                })
            .ToListAsync();

        return rows
            .Select(r => new AnomalyDto(r.Id, r.TransactionId, r.AnomalyType, r.Score, r.Description, r.DetectedAt, r.IsReviewed,
                r.TransactionDate, r.Amount, r.Currency, r.TransactionDescription, r.CategoryId, r.CategoryName))
            .ToList();
    }
}

public sealed class AnomalyRepository(ApplicationDbContext db) : IAnomalyRepository
{
    public async Task<IReadOnlyList<FinancialAnomaly>> GetAllAsync(Guid userId)
        => await db.FinancialAnomalies.Where(a => a.UserId == userId).ToListAsync();

    public async Task<FinancialAnomaly?> GetByIdAsync(Guid userId, Guid id)
        => await db.FinancialAnomalies.FirstOrDefaultAsync(a => a.UserId == userId && a.Id == id);

    public async Task SaveAsync(IReadOnlyList<FinancialAnomaly> toAdd, IReadOnlyList<FinancialAnomaly> toRemove)
    {
        // Egyetlen SaveChanges: a hozzáadás, törlés és a követett entitások módosítása együtt, atomikusan mentődik.
        db.FinancialAnomalies.AddRange(toAdd);
        db.FinancialAnomalies.RemoveRange(toRemove);
        await db.SaveChangesAsync();
    }
}
