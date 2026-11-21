using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Analytics.Recurring;

/// <summary>
/// UC-24: a felhasználó korábbi kiadásainak elemzése, az ismétlődő minták felismerése és mentése.
/// Az új eredmény lecseréli a felhasználó korábbi eredményeit. A felismerés becslés, nem megerősített előfizetés.
/// </summary>
public sealed class DetectRecurringHandler(
    ICurrentUser currentUser,
    IAnalyticsQueries queries,
    IRecurringTransactionRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<RecurringTransactionsDto>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        var expenses = await queries.GetExpensesSinceAsync(userId, today.AddDays(-RecurrenceDetector.LookbackDays));
        var detected = RecurrenceDetector.Detect(expenses, today);

        var entities = detected
            .Select(d => RecurringTransaction.Create(
                Guid.NewGuid(),
                userId,
                d.Description,
                d.CategoryId,
                d.ExpectedAmount,
                d.Currency,
                d.Frequency,
                d.LastOccurrence,
                d.Confidence,
                now))
            .ToList();

        await repository.ReplaceAsync(userId, entities);

        var rows = await queries.GetRecurringAsync(userId);
        var lastChange = await queries.GetLastTransactionChangeAsync(userId);
        return Result<RecurringTransactionsDto>.Ok(RecurringMapper.ToDto(rows, lastChange));
    }
}
