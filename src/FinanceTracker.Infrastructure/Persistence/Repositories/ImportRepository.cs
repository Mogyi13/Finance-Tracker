using FinanceTracker.Application.Features.Imports;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class ImportRepository(ApplicationDbContext db) : IImportRepository
{
    // Egy lekérdezésbe kerülő ujjlenyomatok legnagyobb száma (az IN lista mérete miatt).
    private const int FingerprintChunkSize = 1000;

    public async Task<IReadOnlyDictionary<string, int>> GetExistingFingerprintCountsAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        IReadOnlyCollection<string> fingerprints)
    {
        var counts = new Dictionary<string, int>();
        if (fingerprints.Count == 0)
        {
            return counts;
        }

        // 1) Tárolt ujjlenyomat alapján: az (UserId, Fingerprint) indexen keresünk, csak a saját tranzakciók között.
        foreach (var chunk in fingerprints.Distinct().Chunk(FingerprintChunkSize))
        {
            var stored = await db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Fingerprint != null && chunk.Contains(t.Fingerprint))
                .GroupBy(t => t.Fingerprint!)
                .Select(g => new { Fingerprint = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in stored)
            {
                counts[item.Fingerprint] = counts.GetValueOrDefault(item.Fingerprint) + item.Count;
            }
        }

        // 2) A még ujjlenyomat nélküli (régebben rögzített) tranzakciók: a dátumtartományban menet közben számoljuk.
        var wanted = fingerprints.ToHashSet(StringComparer.Ordinal);
        var legacy = await db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Fingerprint == null && t.TransactionDate >= from && t.TransactionDate <= to)
            .Select(t => new { t.TransactionDate, t.Type, t.Amount, t.Currency, t.Description })
            .ToListAsync();

        foreach (var row in legacy)
        {
            var fingerprint = TransactionFingerprint.Compute(row.TransactionDate, row.Type, row.Amount, row.Currency, row.Description);
            if (wanted.Contains(fingerprint))
            {
                counts[fingerprint] = counts.GetValueOrDefault(fingerprint) + 1;
            }
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

    public async Task<(IReadOnlyList<ImportBatchDto> Items, int TotalCount)> GetBatchesAsync(Guid userId, int skip, int take)
    {
        var query = db.ImportBatches.AsNoTracking().Where(b => b.UserId == userId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .Skip(skip)
            .Take(take)
            .Select(b => new ImportBatchDto(
                b.Id,
                b.FileName,
                b.Status,
                b.TotalRows,
                b.SuccessfulRows,
                b.FailedRows,
                b.DuplicateRows,
                db.ImportErrors.Count(e => e.ImportBatchId == b.Id),
                b.CreatedAt,
                b.CompletedAt))
            .ToListAsync();

        return (items, total);
    }

    public async Task<ImportBatchDto?> GetBatchAsync(Guid userId, Guid batchId)
        => await db.ImportBatches
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.Id == batchId)
            .Select(b => new ImportBatchDto(
                b.Id,
                b.FileName,
                b.Status,
                b.TotalRows,
                b.SuccessfulRows,
                b.FailedRows,
                b.DuplicateRows,
                db.ImportErrors.Count(e => e.ImportBatchId == b.Id),
                b.CreatedAt,
                b.CompletedAt))
            .FirstOrDefaultAsync();

    public async Task<(IReadOnlyList<ImportErrorDto> Items, int TotalCount)> GetErrorsAsync(
        Guid userId, Guid batchId, int skip, int take)
    {
        // A tulajdonos ellenőrzése az importfolyamaton keresztül történik.
        var query = db.ImportErrors
            .AsNoTracking()
            .Where(e => e.ImportBatchId == batchId
                        && db.ImportBatches.Any(b => b.Id == batchId && b.UserId == userId));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.RowNumber)
            .ThenBy(e => e.ErrorCode)
            .Skip(skip)
            .Take(take)
            .Select(e => new ImportErrorDto(e.RowNumber, e.ErrorCode, e.ErrorMessage))
            .ToListAsync();

        return (items, total);
    }
}
