using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Common.Pagination;

namespace FinanceTracker.Application.Features.Transactions.GetTransactions;

/// <summary>UC-06: a bejelentkezett felhasználó tranzakcióinak listázása.</summary>
public sealed class GetTransactionsHandler(ICurrentUser currentUser, ITransactionQueries queries)
{
    public async Task<Result<PagedResult<TransactionDto>>> HandleAsync(GetTransactionsQuery query)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var errors = GetTransactionsValidator.Validate(query);
        if (errors.Any)
        {
            return errors.ToError();
        }

        // A felhasználó azonosítója mindig a hitelesített kontextusból jön, soha nem a kérésből.
        var page = await queries.GetPageAsync(userId, query);
        return Result<PagedResult<TransactionDto>>.Ok(page);
    }
}