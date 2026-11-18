using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
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
}
