namespace FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;

/// <summary>UC-22: a kategóriánkénti kiadás-összesítésekből pénznemenkénti bontást készít (tiszta függvény).</summary>
public static class CategoryBreakdownCalculator
{
    public const string UncategorizedName = "Kategorizálatlan";

    public static IReadOnlyList<CurrencyCategoryBreakdownDto> Build(IEnumerable<CategoryTotal> totals)
        => totals
            .GroupBy(t => t.Currency, StringComparer.Ordinal)
            .Select(g =>
            {
                // Egy kategória egy pénznemben legfeljebb egyszer szerepel; az összevonás védi a nem várt, ismétlődő sorokat.
                var merged = g
                    .GroupBy(t => t.CategoryId)
                    .Select(x => (
                        CategoryId: x.Key,
                        Name: x.Key is null ? UncategorizedName : x.Select(t => t.CategoryName).FirstOrDefault(n => n is not null) ?? UncategorizedName,
                        Amount: x.Sum(t => t.Amount),
                        Count: x.Sum(t => t.Count)))
                    .ToList();

                var total = merged.Sum(m => m.Amount);
                var categories = merged
                    .OrderByDescending(m => m.Amount)
                    .ThenBy(m => m.Name, StringComparer.CurrentCulture)
                    .Select(m => new CategoryExpenseDto(
                        m.CategoryId,
                        m.Name,
                        m.Amount,
                        total == 0 ? 0m : Math.Round(m.Amount / total, 4),
                        m.Count))
                    .ToList();

                return new CurrencyCategoryBreakdownDto(g.Key, total, merged.Sum(m => m.Count), categories);
            })
            .OrderByDescending(c => c.TotalExpense)
            .ThenBy(c => c.Currency, StringComparer.Ordinal)
            .ToList();
}
