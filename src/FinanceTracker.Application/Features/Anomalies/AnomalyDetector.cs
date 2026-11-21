using System.Globalization;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Anomalies;

/// <summary>Egy kiadás az anomáliafelismeréshez.</summary>
public sealed record AnomalyCandidate(Guid TransactionId, DateOnly Date, decimal Amount, string Currency, Guid? CategoryId, string? CategoryName);

public sealed record DetectedAnomaly(Guid TransactionId, AnomalyType Type, decimal Score, string Description);

/// <summary>
/// Az elemzés eredménye. A <see cref="TestedTransactionIds"/> azok a tranzakciók, amelyeket ténylegesen megvizsgáltunk;
/// az ezekre korábban mentett, de most már nem észlelt anomáliák törölhetők.
/// </summary>
public sealed record AnomalyDetectionResult(
    IReadOnlyList<DetectedAnomaly> Anomalies,
    int AmountGroupsEvaluated,
    int MonthlyGroupsEvaluated,
    IReadOnlySet<Guid> TestedTransactionIds)
{
    /// <summary>Volt-e legalább egy csoport, amelyben elég előzmény volt a megbízható alapvonalhoz.</summary>
    public bool HadEnoughData => AmountGroupsEvaluated + MonthlyGroupsEvaluated > 0;
}

/// <summary>
/// UC-25: statisztikai anomáliafelismerés két módszerrel.
///
/// 1. <b>Szokatlanul magas összeg</b>: a kiadásokat (kategória, pénznem) csoportokba soroljuk (a kategória nélküli kiadásnak nincs
///    értelmezhető társcsoportja, ezért kimarad). A vizsgált tranzakció z-pontszámát a csoport TÖBBI tranzakciójából számított átlaggal és
///    (minta-)szórással határozzuk meg: z = (x − átlag) / szórás. A vizsgált tételt nem vonjuk be az alapvonalba, mert egy kiugró érték
///    megnövelné a szórást és elfedné saját magát (n tételnél a bevont z legfeljebb (n−1)/√n lehetne). Anomália, ha z ≥ küszöb és az
///    összeg legalább a megadott relatív mértékkel meghaladja az átlagot.
/// 2. <b>Kategóriaeltérés</b>: a kategória havi kiadásösszegeit (a hiányzó hónapok 0-val) hasonlítjuk a többi hónap átlagához ugyanígy.
///    A jelzés a hónap legnagyobb tranzakciójához kötődik; ha az már „szokatlanul magas összeg” jelzést kapott, nem jelezzük kétszer.
///
/// Kevés előzményadatnál nem jelzünk: a minimális alapvonal-méret a beállításokban van, és az eredmény jelzi, ha egyik csoportban sem volt elég adat.
/// Az eredmény statisztikai jelzés, nem hibát vagy csalást jelent (BR-11).
/// </summary>
public static class AnomalyDetector
{
    // A magyar formátum, ICU nélküli (invariáns globalizációs módú) környezetben a független kultúra.
    private static readonly CultureInfo Hu = CreateCulture();

    private static CultureInfo CreateCulture()
    {
        try
        {
            return new CultureInfo("hu-HU");
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    public static AnomalyDetectionResult Detect(
        IEnumerable<AnomalyCandidate> expenses,
        DateOnly asOf,
        AnomalySettings settings,
        double? thresholdOverride = null)
    {
        var amountThreshold = thresholdOverride ?? settings.AmountZScoreThreshold;
        var categoryThreshold = thresholdOverride ?? settings.CategoryZScoreThreshold;

        var lookbackStart = new DateOnly(asOf.Year, asOf.Month, 1).AddMonths(-settings.LookbackMonths);
        var groups = expenses
            .Where(e => e.CategoryId is not null && e.Amount > 0 && e.Date >= lookbackStart && e.Date <= asOf)
            .GroupBy(e => (e.CategoryId, e.Currency))
            .ToList();

        var anomalies = new List<DetectedAnomaly>();
        var tested = new HashSet<Guid>();
        var amountGroups = 0;
        var monthlyGroups = 0;

        foreach (var group in groups)
        {
            var items = group.ToList();

            if (DetectHighAmounts(items, asOf, settings, amountThreshold, anomalies, tested))
            {
                amountGroups++;
            }

            if (DetectCategoryDeviation(items, asOf, settings, categoryThreshold, anomalies, tested))
            {
                monthlyGroups++;
            }
        }

        var ordered = anomalies
            .OrderByDescending(a => a.Score)
            .ThenBy(a => a.TransactionId)
            .ToList();

        return new AnomalyDetectionResult(ordered, amountGroups, monthlyGroups, tested);
    }

    private static bool DetectHighAmounts(
        List<AnomalyCandidate> items,
        DateOnly asOf,
        AnomalySettings settings,
        double threshold,
        List<DetectedAnomaly> anomalies,
        HashSet<Guid> tested)
    {
        // A vizsgált tétel nélkül is legyen elég tranzakció az alapvonalhoz.
        if (items.Count - 1 < settings.MinBaselineTransactions)
        {
            return false;
        }

        var windowStart = asOf.AddDays(-settings.AnalysisWindowDays);
        double sum = 0, sumSquares = 0;
        foreach (var item in items)
        {
            var x = (double)item.Amount;
            sum += x;
            sumSquares += x * x;
        }

        foreach (var candidate in items.Where(i => i.Date >= windowStart))
        {
            tested.Add(candidate.TransactionId);

            var x = (double)candidate.Amount;
            var count = items.Count - 1;
            var stats = Baseline(sum - x, sumSquares - x * x, count);
            if (stats is not { } s)
            {
                continue;
            }

            var z = (x - s.Mean) / s.StdDev;
            if (z < threshold || x < s.Mean * (1 + settings.MinRelativeIncrease))
            {
                continue;
            }

            var description =
                $"A(z) {Money(candidate.Amount, candidate.Currency)} összeg {z.ToString("N1", Hu)} szórással magasabb, mint a(z) " +
                $"{candidate.CategoryName ?? "kategória"} kategória szokásos tranzakcióösszege (átlag: {Money((decimal)s.Mean, candidate.Currency)}, " +
                $"{count} másik tranzakció alapján).";

            anomalies.Add(new DetectedAnomaly(candidate.TransactionId, AnomalyType.UnusuallyHighAmount, Round(z), description));
        }

        return true;
    }

    private static bool DetectCategoryDeviation(
        List<AnomalyCandidate> items,
        DateOnly asOf,
        AnomalySettings settings,
        double threshold,
        List<DetectedAnomaly> anomalies,
        HashSet<Guid> tested)
    {
        var currentMonth = new DateOnly(asOf.Year, asOf.Month, 1);
        var first = items.Min(i => new DateOnly(i.Date.Year, i.Date.Month, 1));

        // Az alapvonal a csoport első hónapjától az utolsó lezárt hónapig tart; a hiányzó hónapok 0 kiadást jelentenek.
        var lastComplete = currentMonth.AddMonths(-1);
        var totals = new Dictionary<DateOnly, double>();
        for (var month = first; month <= currentMonth; month = month.AddMonths(1))
        {
            totals[month] = 0;
        }

        foreach (var item in items)
        {
            totals[new DateOnly(item.Date.Year, item.Date.Month, 1)] += (double)item.Amount;
        }

        var completeMonths = totals.Keys.Where(m => m <= lastComplete).ToList();
        var evaluated = false;

        for (var offset = 0; offset < settings.AnalysisWindowMonths; offset++)
        {
            var month = currentMonth.AddMonths(-offset);
            if (!totals.ContainsKey(month))
            {
                continue;
            }

            var baselineMonths = completeMonths.Where(m => m != month).ToList();
            if (baselineMonths.Count < settings.MinBaselineMonths)
            {
                continue;
            }

            evaluated = true;
            var monthItems = items.Where(i => new DateOnly(i.Date.Year, i.Date.Month, 1) == month).ToList();
            foreach (var item in monthItems)
            {
                tested.Add(item.TransactionId);
            }

            if (monthItems.Count == 0)
            {
                continue;
            }

            var values = baselineMonths.Select(m => totals[m]).ToList();
            var stats = Baseline(values.Sum(), values.Sum(v => v * v), values.Count);
            if (stats is not { } s)
            {
                continue;
            }

            var total = totals[month];
            var z = (total - s.Mean) / s.StdDev;
            if (z < threshold || total < s.Mean * (1 + settings.MinRelativeIncrease))
            {
                continue;
            }

            var representative = monthItems.OrderByDescending(i => i.Amount).ThenBy(i => i.TransactionId).First();

            // Ha a havi eltérést egyetlen, már jelzett kiugró tétel okozza, a második jelzés felesleges ismétlés lenne.
            if (anomalies.Any(a => a.TransactionId == representative.TransactionId && a.Type == AnomalyType.UnusuallyHighAmount))
            {
                continue;
            }

            var description =
                $"A(z) {representative.CategoryName ?? "kategória"} kategória {month.Year}. {month.Month:00}. havi kiadása " +
                $"({Money((decimal)total, representative.Currency)}) {z.ToString("N1", Hu)} szórással magasabb a szokásos havi szintnél " +
                $"(átlag: {Money((decimal)s.Mean, representative.Currency)}, {baselineMonths.Count} másik hónap alapján).";

            anomalies.Add(new DetectedAnomaly(representative.TransactionId, AnomalyType.CategoryDeviation, Round(z), description));
        }

        return evaluated;
    }

    /// <summary>Átlag és minta-szórás az összegekből; null, ha a szórás nulla (nincs mihez viszonyítani) vagy kevés az adat.</summary>
    private static (double Mean, double StdDev)? Baseline(double sum, double sumSquares, int count)
    {
        if (count < 2)
        {
            return null;
        }

        var mean = sum / count;
        var variance = (sumSquares - sum * sum / count) / (count - 1);
        if (variance <= 1e-9)
        {
            return null;
        }

        return (mean, Math.Sqrt(variance));
    }

    private static decimal Round(double z) => Math.Round((decimal)z, 4);

    private static string Money(decimal amount, string currency) => $"{amount.ToString("N2", Hu)} {currency}";
}
