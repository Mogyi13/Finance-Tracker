using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Analytics.Recurring;

/// <summary>UC-24: a legutóbb mentett ismétlődő kiadások megjelenítése (nem számol újra).</summary>
public sealed class GetRecurringHandler(ICurrentUser currentUser, IAnalyticsQueries queries)
{
    public async Task<Result<RecurringTransactionsDto>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var rows = await queries.GetRecurringAsync(userId);
        var lastChange = await queries.GetLastTransactionChangeAsync(userId);
        return Result<RecurringTransactionsDto>.Ok(RecurringMapper.ToDto(rows, lastChange));
    }
}
