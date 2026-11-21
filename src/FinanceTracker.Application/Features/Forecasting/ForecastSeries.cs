using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Forecasting;

/// <summary>A lezárt hónapok idősorainak összeállítása pénznemenként.</summary>
public static class ForecastSeries
{
    /// <summary>Az utolsó lezárt hónap első napja (az aktuális hónap még nem teljes, ezért nem része az előzménynek).</summary>
    public static DateOnly LastCompleteMonth(DateOnly today) => new DateOnly(today.Year, today.Month, 1).AddMonths(-1);

    public static DateOnly FirstMonthNeeded(DateOnly today, int maxHistoryMonths) => LastCompleteMonth(today).AddMonths(-(maxHistoryMonths - 1));

    /// <summary>
    /// Pénznemenkénti havi idősor az első adatot tartalmazó hónaptól az utolsó lezárt hónapig; a közbeeső üres hónapok 0-t kapnak.
    /// Kiadásnál a havi kiadás, nettó cashflow-nál a bevétel és kiadás különbsége az érték.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<MonthPointDto>> Build(
        IEnumerable<MonthlyTotal> totals, ForecastType type, DateOnly today, int maxHistoryMonths)
    {
        var last = LastCompleteMonth(today);
        var first = FirstMonthNeeded(today, maxHistoryMonths);

        var result = new SortedDictionary<string, IReadOnlyList<MonthPointDto>>(StringComparer.Ordinal);
        foreach (var currency in totals.Where(t => t.Month >= first && t.Month <= last).GroupBy(t => t.Currency))
        {
            var byMonth = currency
                .GroupBy(t => t.Month)
                .ToDictionary(
                    g => g.Key,
                    g => type == ForecastType.MonthlyExpenses
                        ? g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
                        : g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount)
                          - g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount));

            var start = byMonth.Keys.Min();
            var points = new List<MonthPointDto>();
            for (var month = start; month <= last; month = month.AddMonths(1))
            {
                points.Add(new MonthPointDto(month, byMonth.TryGetValue(month, out var v) ? v : 0m));
            }

            result[currency.Key] = points;
        }

        return result;
    }
}
