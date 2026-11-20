using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Common.Pagination;

namespace FinanceTracker.Application.Features.Imports.GetImports;

/// <summary>UC-14: a bejelentkezett felhasználó korábbi importjai.</summary>
public sealed class GetImportsHandler(ICurrentUser currentUser, IImportRepository imports)
{
    public async Task<Result<PagedResult<ImportBatchDto>>> HandleAsync(GetImportsQuery query)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var errors = new FieldErrors();
        if (query.Page < 1)
        {
            errors.Add("page", "Az oldalszám legalább 1 lehet.");
        }

        if (query.PageSize is < 1 or > GetImportsQuery.MaxPageSize)
        {
            errors.Add("pageSize", $"Az oldalméret 1 és {GetImportsQuery.MaxPageSize} között lehet.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        // A felhasználó azonosítója mindig a hitelesített kontextusból jön.
        var (items, total) = await imports.GetBatchesAsync(userId, (query.Page - 1) * query.PageSize, query.PageSize);
        return Result<PagedResult<ImportBatchDto>>.Ok(new PagedResult<ImportBatchDto>(items, query.Page, query.PageSize, total));
    }
}
