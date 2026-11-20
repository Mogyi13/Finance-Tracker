using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Analytics.GetOverview;

/// <summary>UC-21: az adatbázisból érkező összesítésekből pénznemenkénti áttekintést készít (tiszta függvény).</summary>
public static class OverviewCalculator
{
    public static IReadOnlyList<CurrencyOverviewDto> Build(IEnumerable<TypeTotal> totals)
        => totals
            .GroupBy(t => t.Currency, StringComparer.Ordinal)
            .Select(g =>
            {
                var income = g.Where(t => t.Type == TransactionType.Income).ToList();
                var expense = g.Where(t => t.Type == TransactionType.Expense).ToList();
                var totalIncome = income.Sum(t => t.Amount);
                var totalExpense = expense.Sum(t => t.Amount);
                var incomeCount = income.Sum(t => t.Count);
                var expenseCount = expense.Sum(t => t.Count);

                return new CurrencyOverviewDto(
                    g.Key,
                    totalIncome,
                    totalExpense,
                    totalIncome - totalExpense,
                    incomeCount,
                    expenseCount,
                    incomeCount + expenseCount);
            })
            .OrderByDescending(c => c.TransactionCount)
            .ThenBy(c => c.Currency, StringComparer.Ordinal)
            .ToList();
}
