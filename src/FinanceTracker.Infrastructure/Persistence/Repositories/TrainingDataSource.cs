using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

/// <summary>
/// A tanítóadat forrása: a felhasználók által KÉZZEL kategorizált tranzakciók, ha a kategória rendszerkategória
/// (a modell minden felhasználó között közös, ezért a felhasználók saját kategóriáira nem taníthat).
/// Az automatikusan (szabály vagy modell) besorolt tranzakciók kimaradnak, hogy a modell ne a saját korábbi
/// tippjein tanuljon (visszacsatolásos hurok). A kézi javítások (UC-17) mind ManuallyAssigned állapotúak, így benne vannak.
/// </summary>
public sealed class TrainingDataSource(ApplicationDbContext db) : ITrainingDataSource
{
    public async Task<IReadOnlyList<TrainingSample>> GetSamplesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await (
            from t in db.Transactions.AsNoTracking()
            join c in db.Categories.AsNoTracking() on t.CategoryId equals (Guid?)c.Id
            where t.CategorizationStatus == CategorizationStatus.ManuallyAssigned
                  && c.UserId == null
                  && t.Description != null
            select new { t.Id, t.UserId, t.Type, t.Description, CategoryId = c.Id, CategoryType = c.Type })
            .ToListAsync(cancellationToken);

        // A kategória típusának egyeznie kell a tranzakcióéval (az adatbázisban ez a tárolt érték, ezért memóriában hasonlítjuk).
        return rows
            .Where(r => (byte)r.CategoryType == (byte)r.Type)
            .Select(r => new TrainingSample(r.Id, r.UserId, r.Type, r.Description, r.CategoryId))
            .ToList();
    }
}
