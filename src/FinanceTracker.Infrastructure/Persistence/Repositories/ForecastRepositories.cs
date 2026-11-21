using FinanceTracker.Application.Features.Forecasting;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

/// <summary>UC-28 olvasási oldali lekérdezései; minden lekérdezés a felhasználó azonosítójára szűr (BR-14).</summary>
public sealed class ForecastQueries(ApplicationDbContext db) : IForecastQueries
{
    public async Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(Guid userId, DateOnly from, DateOnly to)
    {
        var rows = await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month, t.Type, t.Currency })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Type, g.Key.Currency, Amount = g.Sum(t => t.Amount) })
            .ToListAsync();

        return rows.Select(r => new MonthlyTotal(new DateOnly(r.Year, r.Month, 1), r.Type, r.Currency, r.Amount)).ToList();
    }

    public async Task<IReadOnlyList<ForecastResult>> GetSavedAsync(Guid userId, ForecastType type)
        => await db.ForecastResults.AsNoTracking()
            .Where(f => f.UserId == userId && f.ForecastType == type)
            .OrderBy(f => f.PeriodStart)
            .ToListAsync();
}

public sealed class ForecastRepository(ApplicationDbContext db) : IForecastRepository
{
    public async Task ReplaceAsync(Guid userId, ForecastType type, IReadOnlyList<ForecastResult> items)
    {
        // A régi eredmények törlése és az újak mentése egy tranzakcióban történik, így hiba esetén a korábbi eredmény megmarad.
        await using var transaction = await db.Database.BeginTransactionAsync();

        await db.ForecastResults
            .Where(f => f.UserId == userId && f.ForecastType == type)
            .ExecuteDeleteAsync();

        db.ForecastResults.AddRange(items);
        await db.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}
