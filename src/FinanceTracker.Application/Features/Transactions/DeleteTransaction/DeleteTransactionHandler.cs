using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Transactions.DeleteTransaction;

/// <summary>UC-08: tranzakció törlése. A megerősítést a felhasználói felület kéri be.</summary>
public sealed class DeleteTransactionHandler(
    ICurrentUser currentUser,
    ITransactionRepository transactions,
    IDerivedDataInvalidator derivedData)
{
    public async Task<Result<Unit>> HandleAsync(Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var transaction = await transactions.GetByIdAsync(userId, id);
        if (transaction is null)
        {
            return Error.NotFound("A tranzakció nem található.");
        }

        var date = transaction.TransactionDate;

        await transactions.DeleteAsync(transaction);
        await derivedData.InvalidateAsync(userId, new[] { date });

        return Result<Unit>.Ok(Unit.Value);
    }
}
