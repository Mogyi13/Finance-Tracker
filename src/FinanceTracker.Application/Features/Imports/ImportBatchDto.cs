using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Imports;

/// <summary>
/// UC-14: egy korábbi import összesítője (fájlnév, állapot, sorok száma). A részletes hibák
/// az <see cref="ImportErrorDto"/> listájában, külön lekérdezéssel érhetők el.
/// </summary>
public sealed record ImportBatchDto(
    Guid Id,
    string FileName,
    ImportBatchStatus Status,
    int TotalRows,
    int SuccessfulRows,
    int FailedRows,
    int DuplicateRows,
    int ErrorCount,
    DateTime CreatedAt,
    DateTime? CompletedAt);
