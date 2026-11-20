namespace FinanceTracker.Application.Features.Analytics.Recurring;

internal static class RecurringMapper
{
    public static RecurringTransactionsDto ToDto(IReadOnlyList<RecurringRow> rows, DateTime? lastTransactionChange)
    {
        var items = rows
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.ExpectedAmount)
            .ThenBy(r => r.Description, StringComparer.CurrentCulture)
            .Select(r => new RecurringTransactionDto(
                r.Id,
                r.Description,
                r.CategoryId,
                r.CategoryName,
                r.ExpectedAmount,
                r.Currency,
                r.Frequency,
                r.LastOccurrence,
                RecurrenceDetector.NextExpected(r.LastOccurrence, r.Frequency),
                r.Confidence))
            .ToList();

        var estimates = rows
            .GroupBy(r => r.Currency, StringComparer.Ordinal)
            .Select(g => new MonthlyEstimateDto(
                g.Key,
                Math.Round(g.Sum(r => r.ExpectedAmount * RecurrenceDetector.MonthlyFactor(r.Frequency)), 2)))
            .OrderByDescending(e => e.Amount)
            .ThenBy(e => e.Currency, StringComparer.Ordinal)
            .ToList();

        DateTime? analyzedAt = rows.Count == 0 ? null : rows.Max(r => r.CreatedAt);
        var stale = analyzedAt is { } at && lastTransactionChange is { } change && change > at;

        return new RecurringTransactionsDto(items, estimates, analyzedAt, stale);
    }
}
