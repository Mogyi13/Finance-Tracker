using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Imports.GetImport;

/// <summary>UC-14: egy import összesítője. Más felhasználó importja ugyanúgy „nem található”.</summary>
public sealed class GetImportHandler(ICurrentUser currentUser, IImportRepository imports)
{
    public async Task<Result<ImportBatchDto>> HandleAsync(Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var batch = await imports.GetBatchAsync(userId, id);
        return batch is null
            ? Error.NotFound("Az import nem található.")
            : Result<ImportBatchDto>.Ok(batch);
    }
}
