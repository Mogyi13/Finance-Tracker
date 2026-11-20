namespace FinanceTracker.Web.Services;

/// <summary>Zárt dátumtartomány (mindkét nap beletartozik).</summary>
public readonly record struct DateRange(DateOnly From, DateOnly To)
{
    public bool IsValid => From <= To;
}

/// <summary>Gyorsan választható időszakok az elemző oldalakon.</summary>
public sealed record PeriodPreset(string Key, string Label, Func<DateOnly, DateRange> Resolve)
{
    public static readonly IReadOnlyList<PeriodPreset> All = new[]
    {
        new PeriodPreset("this-month", "Aktuális hónap", today => Month(today, 0)),
        new PeriodPreset("last-month", "Előző hónap", today => Month(today, -1)),
        new PeriodPreset("last-30", "Elmúlt 30 nap", today => new DateRange(today.AddDays(-29), today)),
        new PeriodPreset("this-year", "Idei év", today => new DateRange(new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31))),
        new PeriodPreset("last-year", "Tavalyi év", today => new DateRange(new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31)))
    };

    /// <summary>A hónap első és utolsó napja a mai naptól <paramref name="monthOffset"/> hónappal eltolva.</summary>
    public static DateRange Month(DateOnly today, int monthOffset)
    {
        var first = new DateOnly(today.Year, today.Month, 1).AddMonths(monthOffset);
        return new DateRange(first, first.AddMonths(1).AddDays(-1));
    }
}
