using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Common.Abstractions;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);

    /// <summary>A felhasználó számára elérhető kategóriák: a rendszerkategóriák és a saját kategóriák.</summary>
    Task<IReadOnlyList<Category>> GetAvailableAsync(Guid userId);

    Task AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);

    /// <summary>
    /// Van-e már a felhasználó számára elérhető (rendszer- vagy saját), azonos típusú és nevű kategória.
    /// A név összehasonlítása kis- és nagybetűre érzéketlen. Módosításnál a saját azonosító kihagyható.
    /// </summary>
    Task<bool> NameExistsAsync(Guid userId, string name, CategoryType type, Guid? excludeCategoryId = null);

    /// <summary>Hivatkozik-e a kategóriára tranzakció vagy kategorizálási szabály (később költségkeret is).</summary>
    Task<bool> IsInUseAsync(Guid categoryId);
}
