namespace FinanceTracker.Application.Features.Imports;

/// <summary>
/// Az azonnali (kéréshez kötött) feldolgozás korlátai. A nagyobb fájlok háttérfeldolgozása
/// a Background Processing Service bevezetésével kerül be.
/// </summary>
public static class ImportLimits
{
    public const int MaxFileSizeBytes = 5 * 1024 * 1024;

    public const int MaxDataRows = 5000;

    /// <summary>A válaszban visszaadott hibák és duplikált sorok legnagyobb száma.</summary>
    public const int MaxReportedItems = 100;
}
