using FinanceTracker.Application.Features.Analytics;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class RecurringTransactionRepository(ApplicationDbContext db) : IRecurringTransactionRepository
{
    public async Task ReplaceAsync(Guid userId, IReadOnlyList<RecurringTransaction> items)
    {
        // A régi eredmények törlése és az újak mentése egy tranzakcióban történik, így hiba esetén a korábbi eredmény megmarad.
        await using var transaction = await db.Database.BeginTransactionAsync();

        await db.RecurringTransactions
            .Where(r => r.UserId == userId)
            .ExecuteDeleteAsync();

        db.RecurringTransactions.AddRange(items);
        await db.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}
