using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Common.Pagination;

namespace FinanceTracker.Application.Features.Imports.GetImportErrors;

/// <summary>UC-14: egy import soronkénti hibái sorszám szerint.</summary>
public sealed class GetImportErrorsHandler(ICurrentUser currentUser, IImportRepository imports)
{
    public async Task<Result<PagedResult<ImportErrorDto>>> HandleAsync(Guid id, GetImportErrorsQuery query)
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

        if (query.PageSize is < 1 or > GetImportErrorsQuery.MaxPageSize)
        {
            errors.Add("pageSize", $"Az oldalméret 1 és {GetImportErrorsQuery.MaxPageSize} között lehet.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        // A hibák csak akkor érhetők el, ha az import a felhasználóé.
        if (await imports.GetBatchAsync(userId, id) is null)
        {
            return Error.NotFound("Az import nem található.");
        }

        var (items, total) = await imports.GetErrorsAsync(userId, id, (query.Page - 1) * query.PageSize, query.PageSize);
        return Result<PagedResult<ImportErrorDto>>.Ok(new PagedResult<ImportErrorDto>(items, query.Page, query.PageSize, total));
    }
}
