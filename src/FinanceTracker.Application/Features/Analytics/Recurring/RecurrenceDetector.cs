using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Analytics.Recurring;

/// <summary>Egy felismert, valószínűleg ismétlődő kiadás (még nem mentett).</summary>
public sealed record DetectedRecurrence(
    string Description,
    Guid? CategoryId,
    string Currency,
    decimal ExpectedAmount,
    RecurrenceFrequency Frequency,
    DateOnly LastOccurrence,
    decimal Confidence,
    int OccurrenceCount);

/// <summary>
/// UC-24: ismétlődő kiadások felismerése a korábbi kiadásokból. A módszer szándékosan egyszerű és magyarázható:
///
/// 1. Csoportosítás: pénznem + kategória + egységesített leírás (<see cref="MerchantKey"/>). Leírás nélküli tétel nem csoportosítható.
/// 2. Dátumközök: a csoport tételeit dátum szerint rendezve a szomszédos tételek közti napok számát vizsgáljuk.
///    Egy dátumköz illeszkedik egy gyakorisághoz, ha a névleges napszámtól legfeljebb a megengedett arányban tér el
///    (heti 7 nap ±30%, havi 30 nap ±15%, negyedéves 91 nap ±10%, éves 365 nap ±5%). A legjobban illeszkedő gyakoriság nyer,
///    ha a dátumközök legalább 60%-a illeszkedik. Minimális előfordulás: heti 4, havi és negyedéves 3, éves 2.
/// 3. Összeg: a becsült összeg a csoport összegeinek mediánja; az összegek szórása (relatív) az összegstabilitást adja.
/// 4. Aktualitás: ha az utolsó előfordulás óta több mint 2,5 periódus telt el, a minta valószínűleg megszűnt, ezért nem jelenik meg;
///    1,5 periódus után a bizonyosság csökken (a tétel késik).
/// 5. Bizonyosság = 0,40 × dátumköz-illeszkedés + 0,25 × összegstabilitás + 0,35 × előfordulás-szám súly (7 előfordulásnál teljes),
///    megszorozva az aktualitási tényezővel; 0,99-nél nagyobb nem lehet. A <see cref="MinConfidence"/> alatti eredmény nem kerül mentésre.
///
/// Az eredmény becslés, nem megerősített előfizetés.
/// </summary>
public static class RecurrenceDetector
{
    /// <summary>Az ennél kisebb bizonyosságú minták nem jelennek meg.</summary>
    public const decimal MinConfidence = 0.6m;

    /// <summary>A vizsgált időablak napokban (kb. 30 hónap), hogy az éves ismétlődés is felismerhető legyen.</summary>
    public const int LookbackDays = 913;

    private const double MinIntervalFit = 0.6;
    private const double EndedAfterPeriods = 2.5;
    private const double LateAfterPeriods = 1.5;
    private const double LateFactor = 0.7;
    private const decimal MaxConfidence = 0.99m;

    private sealed record Rule(RecurrenceFrequency Frequency, int Days, double Tolerance, int MinOccurrences);

    private static readonly Rule[] Rules =
    {
        new(RecurrenceFrequency.Weekly, 7, 0.30, 4),
        new(RecurrenceFrequency.Monthly, 30, 0.15, 3),
        new(RecurrenceFrequency.Quarterly, 91, 0.10, 3),
        new(RecurrenceFrequency.Yearly, 365, 0.05, 2)
    };

    private static readonly int MinGroupSize = Rules.Min(r => r.MinOccurrences);

    public static IReadOnlyList<DetectedRecurrence> Detect(IEnumerable<ExpenseEntry> expenses, DateOnly asOf)
    {
        var since = asOf.AddDays(-LookbackDays);
        var result = new List<DetectedRecurrence>();

        var groups = expenses
            .Where(e => e.Amount > 0 && e.Date >= since)
            .Select(e => (Entry: e, Key: MerchantKey.Normalize(e.Description)))
            .Where(x => x.Key is not null)
            .GroupBy(x => (x.Entry.Currency, x.Entry.CategoryId, Key: x.Key!));

        foreach (var group in groups)
        {
            var entries = group.Select(x => x.Entry).OrderBy(e => e.Date).ToList();
            if (entries.Count < MinGroupSize)
            {
                continue;
            }

            var detected = Evaluate(entries, asOf);
            if (detected is not null)
            {
                result.Add(detected);
            }
        }

        return result
            .OrderByDescending(r => r.Confidence)
            .ThenByDescending(r => r.ExpectedAmount)
            .ThenBy(r => r.Description, StringComparer.CurrentCulture)
            .ToList();
    }

    private static DetectedRecurrence? Evaluate(IReadOnlyList<ExpenseEntry> entries, DateOnly asOf)
    {
        var intervals = new List<int>();
        for (var i = 1; i < entries.Count; i++)
        {
            intervals.Add(entries[i].Date.DayNumber - entries[i - 1].Date.DayNumber);
        }

        // A legjobban illeszkedő gyakoriság kiválasztása (az illeszkedési sávok nem fedik egymást, így egyértelmű).
        Rule? bestRule = null;
        var bestFit = 0.0;
        foreach (var rule in Rules.Where(r => entries.Count >= r.MinOccurrences))
        {
            var fits = intervals.Count(d => Math.Abs(d - rule.Days) <= rule.Days * rule.Tolerance);
            var fit = (double)fits / intervals.Count;
            if (fit > bestFit)
            {
                bestFit = fit;
                bestRule = rule;
            }
        }

        if (bestRule is null || bestFit < MinIntervalFit)
        {
            return null;
        }

        var last = entries[^1].Date;
        var ageDays = Math.Max(0, asOf.DayNumber - last.DayNumber);
        var periodsSinceLast = (double)ageDays / bestRule.Days;
        if (periodsSinceLast > EndedAfterPeriods)
        {
            return null;
        }

        var amounts = entries.Select(e => e.Amount).OrderBy(a => a).ToList();
        var median = Median(amounts);
        var amountScore = AmountStability(amounts);
        var countScore = Math.Min(1.0, (entries.Count - 1) / 6.0);
        var recency = periodsSinceLast > LateAfterPeriods ? LateFactor : 1.0;

        var raw = (0.40 * bestFit + 0.25 * amountScore + 0.35 * countScore) * recency;
        var confidence = Math.Min(MaxConfidence, Math.Round((decimal)raw, 4));
        if (confidence < MinConfidence)
        {
            return null;
        }

        var expected = Math.Round(median, 2);
        if (expected <= 0)
        {
            return null;
        }

        // A jellemző leírás a leggyakoribb eredeti szöveg; egyenlőségnél a legutóbbi.
        var description = entries
            .Select((e, i) => (Text: (e.Description ?? string.Empty).Trim(), Index: i))
            .Where(x => x.Text.Length > 0)
            .GroupBy(x => x.Text, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Max(x => x.Index))
            .First()
            .First()
            .Text;

        return new DetectedRecurrence(
            description,
            entries[0].CategoryId,
            entries[0].Currency,
            expected,
            bestRule.Frequency,
            last,
            confidence,
            entries.Count);
    }

    /// <summary>1 − 2 × (relatív szórás), 0 és 1 közé szorítva: a teljesen azonos összegek 1, a 50% feletti szórás 0.</summary>
    private static double AmountStability(IReadOnlyList<decimal> amounts)
    {
        var mean = amounts.Average();
        if (mean <= 0)
        {
            return 0;
        }

        var variance = amounts.Sum(a => (double)((a - mean) * (a - mean))) / amounts.Count;
        var cv = Math.Sqrt(variance) / (double)mean;
        return Math.Clamp(1 - 2 * cv, 0, 1);
    }

    private static decimal Median(IReadOnlyList<decimal> sorted)
    {
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2m;
    }

    /// <summary>A következő várható előfordulás: az utolsó előfordulás + egy periódus.</summary>
    public static DateOnly NextExpected(DateOnly last, RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Weekly => last.AddDays(7),
        RecurrenceFrequency.Monthly => last.AddMonths(1),
        RecurrenceFrequency.Quarterly => last.AddMonths(3),
        RecurrenceFrequency.Yearly => last.AddYears(1),
        _ => last
    };

    /// <summary>Egy periódus becsült havi értéke (heti × 52/12, negyedéves ÷ 3, éves ÷ 12).</summary>
    public static decimal MonthlyFactor(RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Weekly => 52m / 12m,
        RecurrenceFrequency.Monthly => 1m,
        RecurrenceFrequency.Quarterly => 1m / 3m,
        RecurrenceFrequency.Yearly => 1m / 12m,
        _ => 0m
    };
}
