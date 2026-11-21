namespace FinanceTracker.Application.Features.Analytics.ComparePeriods;

public sealed record PeriodDto(DateOnly From, DateOnly To);

/// <summary>
/// Egy mutató két időszakban. A <see cref="Difference"/> = összehasonlított − alap. A <see cref="PercentChange"/>
/// a különbség az alapérték abszolút értékének százalékában; null, ha az alapérték 0 (ilyenkor a százalék nem értelmezhető).
/// </summary>
public sealed record MetricComparisonDto(decimal Base, decimal Compared, decimal Difference, decimal? PercentChange);

public sealed record CategoryComparisonDto(
    Guid? CategoryId,
    string CategoryName,
    MetricComparisonDto Expense,
    int BaseCount,
    int ComparedCount);

/// <summary>Egy pénznem összehasonlítása. Különböző pénznemek összegei nincsenek összevonva (BR-09).</summary>
public sealed record CurrencyComparisonDto(
    string Currency,
    MetricComparisonDto Income,
    MetricComparisonDto Expense,
    MetricComparisonDto NetCashFlow,
    IReadOnlyList<CategoryComparisonDto> Categories);

public sealed record PeriodComparisonDto(
    PeriodDto Base,
    PeriodDto Compared,
    IReadOnlyList<CurrencyComparisonDto> Currencies);
