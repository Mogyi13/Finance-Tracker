using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Analytics.ComparePeriods;

/// <summary>UC-23: két pénzügyi időszak összehasonlítása.</summary>
public sealed class ComparePeriodsHandler(ICurrentUser currentUser, IAnalyticsQueries queries)
{
    public async Task<Result<PeriodComparisonDto>> HandleAsync(ComparePeriodsQuery query)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var errors = new FieldErrors();
        PeriodRules.Validate(errors, query.BaseFrom, query.BaseTo, "baseFrom", "baseTo");
        PeriodRules.Validate(errors, query.ComparedFrom, query.ComparedTo, "comparedFrom", "comparedTo");
        if (errors.Any)
        {
            return errors.ToError();
        }

        var baseFrom = query.BaseFrom!.Value;
        var baseTo = query.BaseTo!.Value;
        var comparedFrom = query.ComparedFrom!.Value;
        var comparedTo = query.ComparedTo!.Value;

        // A négy lekérdezés egymástól független, de ugyanazt a DbContextet használja, ezért egymás után fut.
        var baseTotals = await queries.GetTotalsAsync(userId, baseFrom, baseTo);
        var comparedTotals = await queries.GetTotalsAsync(userId, comparedFrom, comparedTo);
        var baseCategories = await queries.GetExpenseTotalsByCategoryAsync(userId, baseFrom, baseTo);
        var comparedCategories = await queries.GetExpenseTotalsByCategoryAsync(userId, comparedFrom, comparedTo);

        return Result<PeriodComparisonDto>.Ok(new PeriodComparisonDto(
            new PeriodDto(baseFrom, baseTo),
            new PeriodDto(comparedFrom, comparedTo),
            PeriodComparisonCalculator.Build(baseTotals, comparedTotals, baseCategories, comparedCategories)));
    }
}
