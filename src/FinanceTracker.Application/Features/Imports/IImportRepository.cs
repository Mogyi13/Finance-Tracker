using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Imports;

public interface IImportRepository
{
    /// <summary>
    /// A felhasználó meglévő tranzakcióinak ujjlenyomatai (lásd <c>TransactionFingerprint</c>) a megadott
    /// dátumtartományban, ujjlenyomatonként a darabszámmal.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> GetExistingFingerprintCountsAsync(Guid userId, DateOnly from, DateOnly to);

    /// <summary>Az import eredményének (összesítő, hibák, új tranzakciók) mentése egyetlen egységként.</summary>
    Task SaveAsync(
        ImportBatch batch,
        IReadOnlyCollection<ImportError> errors,
        IReadOnlyCollection<Transaction> transactions);
}
