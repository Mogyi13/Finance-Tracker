using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class MlModelRepository(ApplicationDbContext db) : IMlModelRepository
{
    public async Task<MlModelVersion?> GetActiveAsync()
        => await db.MlModelVersions.AsNoTracking().FirstOrDefaultAsync(m => m.IsActive);

    public async Task<MlModelVersion?> GetByIdAsync(Guid id)
        => await db.MlModelVersions.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IReadOnlyList<MlModelVersion>> GetAllAsync()
        => await db.MlModelVersions.AsNoTracking().OrderByDescending(m => m.CreatedAt).ThenBy(m => m.Id).ToListAsync();

    public async Task AddAsync(MlModelVersion version)
    {
        db.MlModelVersions.Add(version);
        await db.SaveChangesAsync();
    }

    public async Task ActivateAsync(MlModelVersion version, DateTime activatedAtUtc)
    {
        // A részleges egyedi index miatt az előző aktív verziót előbb kell inaktívvá tenni, ezért két mentés egy tranzakcióban.
        await using var transaction = await db.Database.BeginTransactionAsync();

        var currentlyActive = await db.MlModelVersions.Where(m => m.IsActive && m.Id != version.Id).ToListAsync();
        foreach (var other in currentlyActive)
        {
            other.Deactivate();
        }

        await db.SaveChangesAsync();

        version.Activate(activatedAtUtc);
        await db.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task DeactivateAsync(MlModelVersion version)
    {
        version.Deactivate();
        await db.SaveChangesAsync();
    }
}
