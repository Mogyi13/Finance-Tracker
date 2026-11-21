using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Anomalies;

/// <summary>UC-25: egy felismert anomália az érintett tranzakció adataival. Statisztikai jelzés, nem hiba vagy csalás.</summary>
public sealed record AnomalyDto(
    Guid Id,
    Guid TransactionId,
    AnomalyType Type,
    decimal? Score,
    string Description,
    DateTime DetectedAt,
    bool IsReviewed,
    DateOnly TransactionDate,
    decimal Amount,
    string Currency,
    string? TransactionDescription,
    Guid? CategoryId,
    string? CategoryName);

/// <summary>A mentett anomáliák. A <see cref="LastAnalyzedAt"/> null, ha még nem volt elemzés vagy nincs mentett eredmény.</summary>
public sealed record AnomaliesDto(
    IReadOnlyList<AnomalyDto> Items,
    int UnreviewedCount,
    DateTime? LastAnalyzedAt,
    double AmountThreshold,
    double CategoryThreshold);

/// <summary>Az elemzés eredménye: ha nem volt elég előzményadat, a <see cref="Notice"/> ezt egyértelműen jelzi.</summary>
public sealed record AnomalyAnalysisDto(
    AnomaliesDto Anomalies,
    bool EnoughData,
    string? Notice,
    int NewCount);
