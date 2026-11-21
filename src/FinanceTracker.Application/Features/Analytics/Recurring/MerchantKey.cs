using FinanceTracker.Application.Features.Categorization.Ml;

namespace FinanceTracker.Application.Features.Analytics.Recurring;

/// <summary>
/// UC-24: a tranzakcióleírásból egységesített „kereskedőkulcs”, amely alapján az azonos forrású tételek csoportosíthatók.
/// Ugyanazt a normalizálást használja, mint az ML-bemenet (kisbetű, ékezetek elhagyása, számok és írásjelek eldobása),
/// és kihagyja a hónapneveket, hogy a „Közös költség április” és a „Közös költség május” egy csoportba kerüljön.
/// </summary>
public static class MerchantKey
{
    private static readonly HashSet<string> MonthWords = new(StringComparer.Ordinal)
    {
        "januar", "februar", "marcius", "aprilis", "majus", "junius", "julius", "augusztus",
        "szeptember", "oktober", "november", "december",
        "jan", "feb", "marc", "apr", "maj", "jun", "jul", "aug", "szept", "okt", "nov", "dec"
    };

    /// <summary>Az egységesített kulcs, vagy null, ha a leírásból nem marad használható szó (ilyen tétel nem csoportosítható).</summary>
    public static string? Normalize(string? description)
    {
        var tokens = FeatureText.Tokenize(description).Where(t => !MonthWords.Contains(t)).ToList();
        return tokens.Count == 0 ? null : string.Join(' ', tokens);
    }
}
