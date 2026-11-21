using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Budgets;

/// <summary>UC-26: egy költségkeret a kategória nevével.</summary>
public sealed record BudgetDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    string Currency,
    BudgetPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>Létrehozás és módosítás közös adatai.</summary>
public sealed record SaveBudgetCommand(
    Guid CategoryId,
    decimal Amount,
    string? Currency,
    BudgetPeriodType PeriodType,
    DateOnly StartDate,
    DateOnly? EndDate);

/// <summary>UC-27: a keret felhasználtsági szintje.</summary>
public enum BudgetLevel
{
    /// <summary>A figyelmeztetési küszöb alatt.</summary>
    Ok = 0,

    /// <summary>Elérte a figyelmeztetési küszöböt, de a keretet még nem.</summary>
    Warning = 1,

    /// <summary>Pontosan kimerítette a keretet.</summary>
    Reached = 2,

    /// <summary>Túllépte a keretet.</summary>
    Exceeded = 3
}

/// <summary>
/// UC-27: egy költségkeret állapota egy időszakra. A <see cref="Spent"/> csak a keret kategóriájának és pénznemének kiadásait
/// tartalmazza a keret időszakában (BR-12, BR-09). A <see cref="Remaining"/> túllépésnél negatív.
/// </summary>
public sealed record BudgetStatusDto(
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    string Currency,
    BudgetPeriodType PeriodType,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    bool IsActive,
    decimal Amount,
    decimal Spent,
    decimal Remaining,
    decimal UsedRatio,
    BudgetLevel Level);

public sealed record BudgetStatusesDto(DateOnly AsOf, decimal WarningThreshold, IReadOnlyList<BudgetStatusDto> Items, int WarningCount, int ExceededCount);
