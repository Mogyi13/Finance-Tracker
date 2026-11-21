using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Analytics.GetOverview;

/// <summary>UC-21: pénzügyi áttekintés a kiválasztott időszakra.</summary>
public sealed class GetOverviewHandler(ICurrentUser currentUser, IAnalyticsQueries queries, TimeProvider timeProvider)
{
    public async Task<Result<FinancialOverviewDto>> HandleAsync(GetOverviewQuery query)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var (from, to) = query.From is null && query.To is null
            ? PeriodRules.CurrentMonth(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            : (query.From, query.To);

        var errors = new FieldErrors();
        PeriodRules.Validate(errors, from, to, "from", "to");
        if (errors.Any)
        {
            return errors.ToError();
        }

        // A felhasználó azonosítója mindig a hitelesített kontextusból jön.
        var totals = await queries.GetTotalsAsync(userId, from!.Value, to!.Value);
        var currencies = OverviewCalculator.Build(totals);

        return Result<FinancialOverviewDto>.Ok(new FinancialOverviewDto(
            from.Value,
            to.Value,
            currencies.Sum(c => c.TransactionCount),
            currencies));
    }
}
