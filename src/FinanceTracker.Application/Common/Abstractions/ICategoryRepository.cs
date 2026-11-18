using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Common.Abstractions;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);

    /// <summary>A felhasználó számára elérhető kategóriák: a rendszerkategóriák és a saját kategóriák.</summary>
    Task<IReadOnlyList<Category>> GetAvailableAsync(Guid userId);
}
