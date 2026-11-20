using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Egy CSV-fájl feltöltésének és feldolgozásának eredménye. A fájl tartalmát nem tároljuk,
/// csak az összesítést; a hibás sorok az <see cref="ImportError"/> rekordokban vannak.
/// </summary>
public sealed class ImportBatch
{
    public const int MaxFileNameLength = 255;

    // Az EF Core számára.
    private ImportBatch()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public ImportBatchStatus Status { get; private set; }

    /// <summary>A feldolgozott (adatot tartalmazó) sorok száma, a fejléc nélkül.</summary>
    public int TotalRows { get; private set; }

    public int SuccessfulRows { get; private set; }

    public int FailedRows { get; private set; }

    public int DuplicateRows { get; private set; }

    /// <summary>Az import indítása (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public static ImportBatch Start(Guid id, Guid userId, string fileName, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Az import azonosítója nem lehet üres.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("Az importnak tulajdonos felhasználó kell.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException("A fájlnév megadása kötelező.");
        }

        var trimmed = fileName.Trim();

        return new ImportBatch
        {
            Id = id,
            UserId = userId,
            FileName = trimmed.Length <= MaxFileNameLength ? trimmed : trimmed[^MaxFileNameLength..],
            Status = ImportBatchStatus.Processing,
            CreatedAt = createdAtUtc
        };
    }

    /// <summary>
    /// A feldolgozás befejezése. Ha egyetlen sor sem volt sikeres vagy duplikált, az import sikertelen;
    /// ha volt hibás sor, részben sikeres; egyébként sikeres.
    /// </summary>
    public void Complete(int totalRows, int successfulRows, int failedRows, int duplicateRows, DateTime completedAtUtc)
    {
        EnsureProcessing();

        if (totalRows < 0 || successfulRows < 0 || failedRows < 0 || duplicateRows < 0
            || successfulRows + failedRows + duplicateRows != totalRows)
        {
            throw new DomainException("Az importösszesítő számai nem egyeznek.");
        }

        TotalRows = totalRows;
        SuccessfulRows = successfulRows;
        FailedRows = failedRows;
        DuplicateRows = duplicateRows;
        CompletedAt = completedAtUtc;

        Status = successfulRows + duplicateRows == 0
            ? ImportBatchStatus.Failed
            : failedRows > 0
                ? ImportBatchStatus.PartiallyCompleted
                : ImportBatchStatus.Completed;
    }

    /// <summary>A fájl szerkezete miatt nem feldolgozható import (nincs egyetlen sor sem feldolgozva).</summary>
    public void Fail(DateTime completedAtUtc)
    {
        EnsureProcessing();

        Status = ImportBatchStatus.Failed;
        CompletedAt = completedAtUtc;
    }

    private void EnsureProcessing()
    {
        if (Status != ImportBatchStatus.Processing)
        {
            throw new DomainException("Az import már lezárult.");
        }
    }
}
