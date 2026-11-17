using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Application.Features.Transactions.GetTransactions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class TransactionQueries(ApplicationDbContext db) : ITransactionQueries
{
    public async Task<TransactionDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var joined = from t in db.Transactions.AsNoTracking()
                     where t.UserId == userId && t.Id == id
                     join c in db.Categories on t.CategoryId equals (Guid?)c.Id into categories
                     from c in categories.DefaultIfEmpty()
                     select new { Transaction = t, CategoryName = c != null ? c.Name : null };

        return await joined
            .Select(x => new TransactionDto(
                x.Transaction.Id,
                x.Transaction.Type,
                x.Transaction.Amount,
                x.Transaction.Currency,
                x.Transaction.Description,
                x.Transaction.TransactionDate,
                x.Transaction.CategoryId,
                x.CategoryName,
                x.Transaction.CategorizationStatus,
                x.Transaction.CreatedAt,
                x.Transaction.UpdatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<TransactionDto>> GetPageAsync(Guid userId, GetTransactionsQuery query)
    {
        var totalCount = await db.Transactions.AsNoTracking().CountAsync(t => t.UserId == userId);

        var joined = from t in db.Transactions.AsNoTracking()
                     where t.UserId == userId
                     join c in db.Categories on t.CategoryId equals (Guid?)c.Id into categories
                     from c in categories.DefaultIfEmpty()
                     select new { Transaction = t, CategoryName = c != null ? c.Name : null };

        // A rendezés mindig determinisztikus (dátum, létrehozás, azonosító), így a lapozás stabil.
        var ordered = (query.SortBy, query.SortDirection) switch
        {
            (TransactionSortField.Amount, SortDirection.Ascending) => joined
                .OrderBy(x => x.Transaction.Amount)
                .ThenByDescending(x => x.Transaction.TransactionDate)
                .ThenByDescending(x => x.Transaction.CreatedAt)
                .ThenBy(x => x.Transaction.Id),
            (TransactionSortField.Amount, _) => joined
                .OrderByDescending(x => x.Transaction.Amount)
                .ThenByDescending(x => x.Transaction.TransactionDate)
                .ThenByDescending(x => x.Transaction.CreatedAt)
                .ThenBy(x => x.Transaction.Id),
            (_, SortDirection.Ascending) => joined
                .OrderBy(x => x.Transaction.TransactionDate)
                .ThenBy(x => x.Transaction.CreatedAt)
                .ThenBy(x => x.Transaction.Id),
            _ => joined
                .OrderByDescending(x => x.Transaction.TransactionDate)
                .ThenByDescending(x => x.Transaction.CreatedAt)
                .ThenBy(x => x.Transaction.Id)
        };

        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new TransactionDto(
                x.Transaction.Id,
                x.Transaction.Type,
                x.Transaction.Amount,
                x.Transaction.Currency,
                x.Transaction.Description,
                x.Transaction.TransactionDate,
                x.Transaction.CategoryId,
                x.CategoryName,
                x.Transaction.CategorizationStatus,
                x.Transaction.CreatedAt,
                x.Transaction.UpdatedAt))
            .ToListAsync();

        return new PagedResult<TransactionDto>(items, query.Page, query.PageSize, totalCount);
    }
}
