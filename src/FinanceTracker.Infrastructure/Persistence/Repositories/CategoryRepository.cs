using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext db) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id)
        => await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Category>> GetAvailableAsync(Guid userId)
        => await db.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task AddAsync(Category category)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        if (db.Entry(category).State == EntityState.Detached)
        {
            db.Categories.Update(category);
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }

    // A név összehasonlítása az adatbázis (alapértelmezetten kis- és nagybetűre érzéketlen) rendezése szerint történik.
    public async Task<bool> NameExistsAsync(Guid userId, string name, CategoryType type, Guid? excludeCategoryId = null)
        => await db.Categories
            .AsNoTracking()
            .AnyAsync(c => (c.UserId == null || c.UserId == userId)
                           && c.Type == type
                           && c.Name == name
                           && (excludeCategoryId == null || c.Id != excludeCategoryId));

    public async Task<bool> IsInUseAsync(Guid categoryId)
        => await db.Transactions.AsNoTracking().AnyAsync(t => t.CategoryId == categoryId)
           || await db.CategorizationRules.AsNoTracking().AnyAsync(r => r.CategoryId == categoryId)
           || await db.CategorizationFeedbacks.AsNoTracking().AnyAsync(f => f.CorrectedCategoryId == categoryId || f.PredictedCategoryId == categoryId)
           || await db.Budgets.AsNoTracking().AnyAsync(b => b.CategoryId == categoryId);
}
