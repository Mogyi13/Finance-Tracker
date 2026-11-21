using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Budgets;

/// <summary>Egy költségkeret kiadásának lekérdezése: kategória, pénznem és zárt időszak.</summary>
public sealed record BudgetSpendRequest(Guid BudgetId, Guid CategoryId, string Currency, DateOnly From, DateOnly To);

public interface IBudgetQueries
{
    Task<IReadOnlyList<BudgetDto>> GetAllAsync(Guid userId);

    /// <summary>
    /// A kérésekhez tartozó kiadásösszegek (a kérés azonosítója szerint). Az összegzés az adatbázisban történik,
    /// és csak a felhasználó kiadás típusú tranzakcióira szűr.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetSpentAsync(Guid userId, IReadOnlyList<BudgetSpendRequest> requests);
}

public interface IBudgetRepository
{
    /// <summary>Saját költségkeret; más felhasználóé vagy nem létező esetén null.</summary>
    Task<Budget?> GetByIdAsync(Guid userId, Guid id);

    /// <summary>A felhasználó azonos kategóriájú és pénznemű keretei (az átfedés ellenőrzéséhez).</summary>
    Task<IReadOnlyList<Budget>> GetForCategoryAsync(Guid userId, Guid categoryId, string currency);

    Task AddAsync(Budget budget);

    Task UpdateAsync(Budget budget);

    Task DeleteAsync(Budget budget);
}
