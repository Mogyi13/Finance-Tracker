using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization;

public interface ICategorizationRuleRepository
{
    /// <summary>A felhasználó összes szabálya (aktív és inaktív), kiértékelési sorrendben: prioritás szerint csökkenően, azonos prioritásnál a régebbi elöl.</summary>
    Task<IReadOnlyList<CategorizationRule>> GetAllAsync(Guid userId);

    /// <summary>A felhasználó egy szabálya; más felhasználó szabálya nem érhető el (null).</summary>
    Task<CategorizationRule?> GetByIdAsync(Guid userId, Guid id);

    Task AddAsync(CategorizationRule rule);

    Task UpdateAsync(CategorizationRule rule);

    Task DeleteAsync(CategorizationRule rule);

    /// <summary>Van-e már a felhasználónak azonos kulcsszavú (kis- és nagybetűre érzéketlen), egyezési típusú és kategóriájú szabálya.</summary>
    Task<bool> ExistsAsync(Guid userId, string keyword, RuleMatchType matchType, Guid categoryId, Guid? excludeRuleId = null);
}
