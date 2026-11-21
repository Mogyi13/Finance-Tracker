using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Budgets;

/// <summary>UC-26/27: a költségkeret érvényességi és aktuális időszakának számítása.</summary>
public static class BudgetPeriods
{
    /// <summary>Az utolsó érvényes nap: az egyedi keretnél a záró dátum; havi/heti keretnél az időszak vége a megadott napon.</summary>
    public static (DateOnly Start, DateOnly End) PeriodFor(BudgetPeriodType type, DateOnly reference, DateOnly startDate, DateOnly? endDate)
    {
        switch (type)
        {
            case BudgetPeriodType.Monthly:
                {
                    var first = new DateOnly(reference.Year, reference.Month, 1);
                    return (first, first.AddMonths(1).AddDays(-1));
                }
            case BudgetPeriodType.Weekly:
                {
                    var monday = Budget.NormalizeStart(BudgetPeriodType.Weekly, reference);
                    return (monday, monday.AddDays(6));
                }
            default:
                return (startDate, endDate ?? startDate);
        }
    }

    /// <summary>
    /// A vizsgált időszak: az érvényességi intervallumon belüli referencianapot tartalmazó időszak
    /// (a mai nap, kivéve ha a keret még nem kezdődött el vagy már lejárt: ekkor az első, illetve az utolsó időszaka).
    /// </summary>
    public static (DateOnly Start, DateOnly End, bool IsActive) Resolve(
        BudgetPeriodType type, DateOnly startDate, DateOnly? endDate, DateOnly today)
    {
        var active = today >= startDate && (endDate is null || today <= endDate);
        var reference = today < startDate ? startDate : today;
        if (endDate is { } end && reference > end)
        {
            reference = end;
        }

        var (start, last) = PeriodFor(type, reference, startDate, endDate);
        return (start, last, active);
    }

    /// <summary>Az érvényességi intervallum vége; a határozatlan végű keret „nyitott”.</summary>
    public static DateOnly EffectiveEnd(DateOnly? endDate) => endDate ?? DateOnly.MaxValue;

    /// <summary>Két keret érvényességi intervalluma átfedi-e egymást.</summary>
    public static bool Overlaps(DateOnly startA, DateOnly? endA, DateOnly startB, DateOnly? endB)
        => startA <= EffectiveEnd(endB) && startB <= EffectiveEnd(endA);
}
