using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Imports;

public sealed record ImportErrorDto(int RowNumber, string ErrorCode, string ErrorMessage);

/// <summary>Az import összesítése. A hibák és duplikált sorok listája a megjelenítéshez korlátozott hosszú.</summary>
public sealed record ImportResultDto(
    Guid Id,
    string FileName,
    ImportBatchStatus Status,
    int TotalRows,
    int SuccessfulRows,
    int FailedRows,
    int DuplicateRows,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    IReadOnlyList<ImportErrorDto> Errors,
    int ErrorCount,
    IReadOnlyList<int> DuplicateRowNumbers);
