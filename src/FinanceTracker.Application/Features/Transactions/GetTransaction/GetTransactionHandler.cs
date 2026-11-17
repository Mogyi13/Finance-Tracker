using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Transactions.GetTransaction;

/// <summary>Egy saját tranzakció részletei (UC-07 első lépése).</summary>
public sealed class GetTransactionHandler(ICurrentUser currentUser, ITransactionQueries queries)
{
    public async Task<Result<TransactionDto>> HandleAsync(Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var transaction = await queries.GetByIdAsync(userId, id);
        return transaction is null
            ? Error.NotFound("A tranzakció nem található.")
            : Result<TransactionDto>.Ok(transaction);
    }
}
