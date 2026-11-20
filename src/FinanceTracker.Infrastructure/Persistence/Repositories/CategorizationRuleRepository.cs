using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class CategorizationRuleRepository(ApplicationDbContext db) : ICategorizationRuleRepository
{
    public async Task<IReadOnlyList<CategorizationRule>> GetAllAsync(Guid userId)
        => await db.CategorizationRules
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToListAsync();

    public async Task<CategorizationRule?> GetByIdAsync(Guid userId, Guid id)
        => await db.CategorizationRules.FirstOrDefaultAsync(r => r.UserId == userId && r.Id == id);

    public async Task AddAsync(CategorizationRule rule)
    {
        db.CategorizationRules.Add(rule);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategorizationRule rule)
    {
        if (db.Entry(rule).State == EntityState.Detached)
        {
            db.CategorizationRules.Update(rule);
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(CategorizationRule rule)
    {
        db.CategorizationRules.Remove(rule);
        await db.SaveChangesAsync();
    }

    // A kulcsszó a tárolás előtt már normalizált (szóközök összevonva); a kis- és nagybetűre érzéketlen
    // összehasonlítást az adatbázis alapértelmezett rendezése adja.
    public async Task<bool> ExistsAsync(Guid userId, string keyword, RuleMatchType matchType, Guid categoryId, Guid? excludeRuleId = null)
        => await db.CategorizationRules
            .AsNoTracking()
            .AnyAsync(r => r.UserId == userId
                           && r.Keyword == keyword
                           && r.MatchType == matchType
                           && r.CategoryId == categoryId
                           && (excludeRuleId == null || r.Id != excludeRuleId));
}
