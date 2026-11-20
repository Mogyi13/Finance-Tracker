using System.Globalization;
using FinanceTracker.Application.Features.Categorization.Ml;

namespace FinanceTracker.Web.Services;

/// <summary>A kategóriajavaslatok megbízhatóságának és forrásának egységes megjelenítése.</summary>
public static class PredictionDisplay
{
    public static string Percent(double? confidence)
        => confidence is { } c ? c.ToString("P0", TransactionFormat.Hu) : "–";

    public static string LevelText(ConfidenceLevel? level) => level switch
    {
        ConfidenceLevel.High => "Magas megbízhatóság",
        ConfidenceLevel.Medium => "Közepes megbízhatóság",
        ConfidenceLevel.Low => "Alacsony megbízhatóság",
        _ => string.Empty
    };

    public static string LevelChipClass(ConfidenceLevel? level) => level switch
    {
        ConfidenceLevel.High => "chip-success",
        ConfidenceLevel.Medium => "chip-info",
        ConfidenceLevel.Low => "chip-warning",
        _ => string.Empty
    };

    public static string SourceText(PredictionSource source) => source switch
    {
        PredictionSource.Rule => "Saját szabály",
        PredictionSource.Model => "Modell",
        _ => "–"
    };
}
