using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Forecasting;

/// <summary>Egy hónap és pénznem bevétel- vagy kiadás-összege.</summary>
public sealed record MonthlyTotal(DateOnly Month, TransactionType Type, string Currency, decimal Amount);

public interface IForecastQueries
{
    /// <summary>A felhasználó havi összegei a megadott időszakra (az adatbázisban csoportosítva).</summary>
    Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(Guid userId, DateOnly from, DateOnly to);

    /// <summary>A mentett előrejelzések típus szerint.</summary>
    Task<IReadOnlyList<ForecastResult>> GetSavedAsync(Guid userId, ForecastType type);
}

public interface IForecastRepository
{
    /// <summary>A felhasználó adott típusú korábbi előrejelzéseinek lecserélése az újakra egyetlen tranzakcióban.</summary>
    Task ReplaceAsync(Guid userId, ForecastType type, IReadOnlyList<ForecastResult> items);
}
