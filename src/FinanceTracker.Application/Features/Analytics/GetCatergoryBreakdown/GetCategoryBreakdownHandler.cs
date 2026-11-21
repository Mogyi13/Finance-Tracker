using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;

/// <summary>UC-22: a kiadások kategóriánkénti elemzése a kiválasztott időszakra.</summary>
public sealed class GetCategoryBreakdownHandler(ICurrentUser currentUser, IAnalyticsQueries queries, TimeProvider timeProvider)
{
    public async Task<Result<CategoryBreakdownDto>> HandleAsync(GetCategoryBreakdownQuery query)
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

        var totals = await queries.GetExpenseTotalsByCategoryAsync(userId, from!.Value, to!.Value);

        return Result<CategoryBreakdownDto>.Ok(new CategoryBreakdownDto(
            from.Value,
            to.Value,
            CategoryBreakdownCalculator.Build(totals)));
    }
}
