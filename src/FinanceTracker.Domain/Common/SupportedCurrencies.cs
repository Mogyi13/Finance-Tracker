namespace FinanceTracker.Domain.Common;

/// <summary>
/// A rögzíthető pénznemek (ISO 4217 kódok). Az első verzió nem végez devizaátváltást,
/// ezért a lista bővítése egyetlen helyen, itt történik.
/// </summary>
public static class SupportedCurrencies
{
    public static readonly IReadOnlyList<string> All = new[] { "HUF", "EUR", "USD", "GBP", "CHF", "PLN", "CZK", "RON" };

    public static bool IsSupported(string? currency)
        => currency is not null && All.Contains(currency, StringComparer.Ordinal);
}
