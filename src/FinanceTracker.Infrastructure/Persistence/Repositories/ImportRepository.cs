using FinanceTracker.Application.Features.Imports;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class ImportRepository(ApplicationDbContext db) : IImportRepository
{
    public async Task<IReadOnlyDictionary<string, int>> GetExistingFingerprintCountsAsync(Guid userId, DateOnly from, DateOnly to)
    {
        // Az ujjlenyomatot az alkalmazás számítja (TransactionFingerprint), így a normalizálás egyetlen helyen él.
        // Egyelőre a dátumtartományba eső tranzakciókat olvassuk be; a tárolt, indexelt ujjlenyomat (UC-13) ezt kiváltja.
        var rows = await db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.TransactionDate >= from && t.TransactionDate <= to)
            .Select(t => new { t.TransactionDate, t.Type, t.Amount, t.Currency, t.Description })
            .ToListAsync();

        var counts = new Dictionary<string, int>();
        foreach (var row in rows)
        {
            var fingerprint = TransactionFingerprint.Compute(row.TransactionDate, row.Type, row.Amount, row.Currency, row.Description);
            counts[fingerprint] = counts.GetValueOrDefault(fingerprint) + 1;
        }

        return counts;
    }

    // Egyetlen SaveChanges: az importfolyamat, a hibák és az új tranzakciók egy adatbázis-tranzakcióban mentődnek.
    public async Task SaveAsync(
        ImportBatch batch,
        IReadOnlyCollection<ImportError> errors,
        IReadOnlyCollection<Transaction> transactions)
    {
        db.ImportBatches.Add(batch);
        db.ImportErrors.AddRange(errors);
        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();
    }
}
