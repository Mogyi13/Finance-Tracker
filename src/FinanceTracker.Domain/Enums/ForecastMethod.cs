namespace FinanceTracker.Domain.Enums;

/// <summary>UC-28: az előrejelzési módszer (az adatbázisban tinyint).</summary>
public enum ForecastMethod : byte
{
    MovingAverage = 1,
    LinearRegression = 2
}
