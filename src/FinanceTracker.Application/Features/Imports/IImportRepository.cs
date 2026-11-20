using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Imports;

public interface IImportRepository
{
    /// <summary>
    /// UC-13: a felhasználó meglévő tranzakcióinak darabszáma a megadott ujjlenyomatokra (lásd
    /// <c>TransactionFingerprint</c>). A tárolt ujjlenyomat alapján keres; a még ujjlenyomat nélküli, régebbi
    /// tranzakciókat a <paramref name="from"/>–<paramref name="to"/> dátumtartományban menet közben számolja ki.
    /// Csak a felhasználó saját tranzakcióit vizsgálja.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> GetExistingFingerprintCountsAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        IReadOnlyCollection<string> fingerprints);

    /// <summary>UC-14: a felhasználó importjai, a legutóbbi elöl. Hibák számával.</summary>
    Task<(IReadOnlyList<ImportBatchDto> Items, int TotalCount)> GetBatchesAsync(Guid userId, int skip, int take);

    /// <summary>UC-14: egy import összesítője; más felhasználó importja nem érhető el (null).</summary>
    Task<ImportBatchDto?> GetBatchAsync(Guid userId, Guid batchId);

    /// <summary>UC-14: egy import hibái sorszám szerint; más felhasználó importjára üres.</summary>
    Task<(IReadOnlyList<ImportErrorDto> Items, int TotalCount)> GetErrorsAsync(Guid userId, Guid batchId, int skip, int take);

    /// <summary>Az import eredményének (összesítő, hibák, új tranzakciók) mentése egyetlen egységként.</summary>
    Task SaveAsync(
        ImportBatch batch,
        IReadOnlyCollection<ImportError> errors,
        IReadOnlyCollection<Transaction> transactions);
}
