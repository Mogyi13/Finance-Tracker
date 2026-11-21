using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Anomalies;

public interface IAnomalyQueries
{
    /// <summary>A felhasználó kiadásai a megadott naptól (legfeljebb a legutóbbi néhány százezer), a kategórianévvel.</summary>
    Task<IReadOnlyList<AnomalyCandidate>> GetExpensesSinceAsync(Guid userId, DateOnly from);

    /// <summary>A mentett anomáliák a tranzakció adataival, a legújabb felismerés elöl, ezen belül a nagyobb pontszám elöl.</summary>
    Task<IReadOnlyList<AnomalyDto>> GetAsync(Guid userId, bool includeReviewed);
}

public interface IAnomalyRepository
{
    /// <summary>A felhasználó összes mentett anomáliája (követett entitások, hogy a frissítés menthető legyen).</summary>
    Task<IReadOnlyList<FinancialAnomaly>> GetAllAsync(Guid userId);

    /// <summary>Saját anomália betöltése; más felhasználóé vagy nem létező esetén null.</summary>
    Task<FinancialAnomaly?> GetByIdAsync(Guid userId, Guid id);

    /// <summary>Az új anomáliák hozzáadása, az elavultak törlése és a követett entitások módosításainak mentése egyetlen lépésben.</summary>
    Task SaveAsync(IReadOnlyList<FinancialAnomaly> toAdd, IReadOnlyList<FinancialAnomaly> toRemove);
}
