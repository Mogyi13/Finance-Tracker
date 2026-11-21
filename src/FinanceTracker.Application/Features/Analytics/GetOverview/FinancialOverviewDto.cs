namespace FinanceTracker.Application.Features.Analytics.GetOverview;

/// <summary>
/// UC-21: egy pénznem összesítése. A nettó pénzforgalom = bevétel − kiadás; ez NEM a bankszámla egyenlege
/// (a rendszer nem kezel nyitóegyenleget és számlákat). Különböző pénznemek összegei nincsenek összevonva (BR-09).
/// </summary>
public sealed record CurrencyOverviewDto(
    string Currency,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetCashFlow,
    int IncomeCount,
    int ExpenseCount,
    int TransactionCount);

public sealed record FinancialOverviewDto(
    DateOnly From,
    DateOnly To,
    int TransactionCount,
    IReadOnlyList<CurrencyOverviewDto> Currencies);
