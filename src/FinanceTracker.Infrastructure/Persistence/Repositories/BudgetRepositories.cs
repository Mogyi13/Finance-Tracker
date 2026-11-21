using FinanceTracker.Application.Features.Budgets;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

/// <summary>UC-26 és UC-27 olvasási oldali lekérdezései; minden lekérdezés a felhasználó azonosítójára szűr (BR-14).</summary>
public sealed class BudgetQueries(ApplicationDbContext db) : IBudgetQueries
{
    public async Task<IReadOnlyList<BudgetDto>> GetAllAsync(Guid userId)
    {
        var rows = await (
                from b in db.Budgets.AsNoTracking()
                join c in db.Categories.AsNoTracking() on b.CategoryId equals c.Id
                where b.UserId == userId
                orderby c.Name, b.Currency, b.StartDate
                select new
                {
                    b.Id,
                    b.CategoryId,
                    CategoryName = c.Name,
                    b.Amount,
                    b.Currency,
                    b.PeriodType,
                    b.StartDate,
                    b.EndDate,
                    b.CreatedAt,
                    b.UpdatedAt
                })
            .ToListAsync();

        return rows
            .Select(r => new BudgetDto(r.Id, r.CategoryId, r.CategoryName, r.Amount, r.Currency, r.PeriodType,
                r.StartDate, r.EndDate, r.CreatedAt, r.UpdatedAt))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetSpentAsync(Guid userId, IReadOnlyList<BudgetSpendRequest> requests)
    {
        // Keretenként egy összegző lekérdezés (a keretek száma kicsi); az összegzés az adatbázisban történik.
        var result = new Dictionary<Guid, decimal>();
        foreach (var request in requests)
        {
            var from = request.From;
            var to = request.To;
            var categoryId = request.CategoryId;
            var currency = request.Currency;

            var sum = await db.Transactions.AsNoTracking()
                .Where(t => t.UserId == userId
                            && t.Type == TransactionType.Expense
                            && t.CategoryId == categoryId
                            && t.Currency == currency
                            && t.TransactionDate >= from
                            && t.TransactionDate <= to)
                .SumAsync(t => (decimal?)t.Amount);

            result[request.BudgetId] = sum ?? 0m;
        }

        return result;
    }
}

public sealed class BudgetRepository(ApplicationDbContext db) : IBudgetRepository
{
    public async Task<Budget?> GetByIdAsync(Guid userId, Guid id)
        => await db.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.Id == id);

    public async Task<IReadOnlyList<Budget>> GetForCategoryAsync(Guid userId, Guid categoryId, string currency)
        => await db.Budgets.AsNoTracking()
            .Where(b => b.UserId == userId && b.CategoryId == categoryId && b.Currency == currency)
            .ToListAsync();

    public async Task AddAsync(Budget budget)
    {
        db.Budgets.Add(budget);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Budget budget) => await db.SaveChangesAsync();

    public async Task DeleteAsync(Budget budget)
    {
        db.Budgets.Remove(budget);
        await db.SaveChangesAsync();
    }
}
