namespace FinanceTracker.Domain.Enums;

/// <summary>UC-28: mit becsül az előrejelzés (az adatbázisban tinyint).</summary>
public enum ForecastType : byte
{
    MonthlyExpenses = 1,
    MonthlyNetCashFlow = 2
}
