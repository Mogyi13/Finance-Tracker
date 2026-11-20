using FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;
using FinanceTracker.Application.Features.Analytics.GetOverview;

namespace FinanceTracker.Application.Features.Analytics.ComparePeriods;

/// <summary>UC-23: két időszak összehasonlítása pénznemenként (tiszta függvény).</summary>
public static class PeriodComparisonCalculator
{
    /// <summary>Egy mutató összehasonlítása: abszolút és (ahol értelmezhető) százalékos eltérés.</summary>
    public static MetricComparisonDto Compare(decimal baseValue, decimal comparedValue)
    {
        var difference = comparedValue - baseValue;
        decimal? percent = baseValue == 0 ? null : Math.Round(difference / Math.Abs(baseValue) * 100m, 1);
        return new MetricComparisonDto(baseValue, comparedValue, difference, percent);
    }

    public static IReadOnlyList<CurrencyComparisonDto> Build(
        IEnumerable<TypeTotal> baseTotals,
        IEnumerable<TypeTotal> comparedTotals,
        IEnumerable<CategoryTotal> baseCategories,
        IEnumerable<CategoryTotal> comparedCategories)
    {
        var baseOverview = OverviewCalculator.Build(baseTotals).ToDictionary(c => c.Currency, StringComparer.Ordinal);
        var comparedOverview = OverviewCalculator.Build(comparedTotals).ToDictionary(c => c.Currency, StringComparer.Ordinal);
        var baseBreakdown = CategoryBreakdownCalculator.Build(baseCategories).ToDictionary(c => c.Currency, StringComparer.Ordinal);
        var comparedBreakdown = CategoryBreakdownCalculator.Build(comparedCategories).ToDictionary(c => c.Currency, StringComparer.Ordinal);

        // Azok a pénznemek kerülnek be, amelyek legalább az egyik időszakban előfordulnak.
        var currencies = baseOverview.Keys
            .Union(comparedOverview.Keys, StringComparer.Ordinal)
            .Union(baseBreakdown.Keys, StringComparer.Ordinal)
            .Union(comparedBreakdown.Keys, StringComparer.Ordinal)
            .ToList();

        return currencies
            .Select(currency =>
            {
                baseOverview.TryGetValue(currency, out var b);
                comparedOverview.TryGetValue(currency, out var c);
                baseBreakdown.TryGetValue(currency, out var bb);
                comparedBreakdown.TryGetValue(currency, out var cb);

                return new CurrencyComparisonDto(
                    currency,
                    Compare(b?.TotalIncome ?? 0m, c?.TotalIncome ?? 0m),
                    Compare(b?.TotalExpense ?? 0m, c?.TotalExpense ?? 0m),
                    Compare(b?.NetCashFlow ?? 0m, c?.NetCashFlow ?? 0m),
                    CompareCategories(bb?.Categories, cb?.Categories));
            })
            .OrderByDescending(x => Math.Abs(x.Expense.Base) + Math.Abs(x.Expense.Compared) + Math.Abs(x.Income.Base) + Math.Abs(x.Income.Compared))
            .ThenBy(x => x.Currency, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<CategoryComparisonDto> CompareCategories(
        IReadOnlyList<CategoryExpenseDto>? baseCategories,
        IReadOnlyList<CategoryExpenseDto>? comparedCategories)
    {
        // A kategorizálatlan tételeknek nincs azonosítójuk (null), ezért a szótár kulcsa az üres Guid ennek a jelölésére;
        // valódi kategória azonosítója sosem üres.
        static Guid Key(CategoryExpenseDto c) => c.CategoryId ?? Guid.Empty;

        var left = (baseCategories ?? Array.Empty<CategoryExpenseDto>()).ToDictionary(Key);
        var right = (comparedCategories ?? Array.Empty<CategoryExpenseDto>()).ToDictionary(Key);

        // A kategóriák uniója: az egyik időszakban nem szereplő kategória ott 0 összegű.
        return left.Keys.Union(right.Keys)
            .Select(key =>
            {
                left.TryGetValue(key, out var l);
                right.TryGetValue(key, out var r);
                var name = (r ?? l)!.CategoryName;
                return new CategoryComparisonDto(
                    key == Guid.Empty ? null : key,
                    name,
                    Compare(l?.Amount ?? 0m, r?.Amount ?? 0m),
                    l?.TransactionCount ?? 0,
                    r?.TransactionCount ?? 0);
            })
            .OrderByDescending(c => Math.Abs(c.Expense.Difference))
            .ThenBy(c => c.CategoryName, StringComparer.CurrentCulture)
            .ToList();
    }
}
