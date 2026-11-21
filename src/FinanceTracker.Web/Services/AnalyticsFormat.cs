using FinanceTracker.Application.Features.Analytics.ComparePeriods;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>Az elemzési értékek egységes magyar megjelenítése.</summary>
public static class AnalyticsFormat
{
    public static string Money(decimal amount, string currency)
        => $"{amount.ToString("N2", TransactionFormat.Hu)} {currency}";

    /// <summary>Előjeles összeg (a negatív érték valódi mínuszjellel).</summary>
    public static string SignedMoney(decimal amount, string currency)
        => amount switch
        {
            > 0 => $"+{Money(amount, currency)}",
            < 0 => $"−{Money(Math.Abs(amount), currency)}",
            _ => Money(0, currency)
        };

    /// <summary>A teljes kiadáson belüli arány (0–1 közötti értékből).</summary>
    public static string Share(decimal share) => share.ToString("P1", TransactionFormat.Hu);

    /// <summary>Százalékos eltérés; ha az alapérték 0, a százalék nem értelmezhető.</summary>
    public static string PercentChange(MetricComparisonDto metric)
    {
        if (metric.PercentChange is not { } percent)
        {
            return metric.Difference == 0 ? "–" : "nincs alap";
        }

        var text = Math.Abs(percent).ToString("N1", TransactionFormat.Hu) + "%";
        return percent switch
        {
            > 0 => "+" + text,
            < 0 => "−" + text,
            _ => text
        };
    }

    /// <summary>
    /// Az eltérés színe: a bevétel és a nettó pénzforgalom növekedése kedvező, a kiadás növekedése kedvezőtlen.
    /// </summary>
    public static string DeltaClass(decimal difference, bool higherIsBetter)
    {
        if (difference == 0)
        {
            return "delta-neutral";
        }

        return (difference > 0) == higherIsBetter ? "delta-good" : "delta-bad";
    }

    /// <summary>A százalékos eltérés színe: ha a százalék nem értelmezhető (nincs alap), semleges.</summary>
    public static string PercentClass(MetricComparisonDto metric, bool higherIsBetter)
        => metric.PercentChange is null ? "delta-neutral" : DeltaClass(metric.Difference, higherIsBetter);

    public static string FrequencyText(RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Weekly => "Hetente",
        RecurrenceFrequency.Monthly => "Havonta",
        RecurrenceFrequency.Quarterly => "Negyedévente",
        RecurrenceFrequency.Yearly => "Évente",
        _ => "–"
    };

    public static string ConfidenceText(decimal confidence) => confidence switch
    {
        >= 0.85m => "Magas",
        >= 0.70m => "Közepes",
        _ => "Alacsony"
    };

    public static string ConfidenceChipClass(decimal confidence) => confidence switch
    {
        >= 0.85m => "chip-success",
        >= 0.70m => "chip-info",
        _ => "chip-warning"
    };
}
