using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(ApplicationDbContext db) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid userId, Guid id)
        => await db.Transactions.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == id);

    public async Task AddAsync(Transaction transaction)
    {
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        if (db.Entry(transaction).State == EntityState.Detached)
        {
            db.Transactions.Update(transaction);
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync();
    }
}
