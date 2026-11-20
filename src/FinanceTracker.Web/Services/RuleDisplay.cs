using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>A kategorizálási szabályok magyar megjelenítése.</summary>
public static class RuleDisplay
{
    public static string MatchTypeName(RuleMatchType matchType) => matchType switch
    {
        RuleMatchType.Exact => "Pontos egyezés",
        RuleMatchType.Contains => "Tartalmazza",
        RuleMatchType.StartsWith => "Ezzel kezdődik",
        _ => matchType.ToString()
    };

    public static string MatchTypeHint(RuleMatchType matchType) => matchType switch
    {
        RuleMatchType.Exact => "A leírás pontosan megegyezik a kulcsszóval.",
        RuleMatchType.Contains => "A leírás tartalmazza a kulcsszót.",
        RuleMatchType.StartsWith => "A leírás a kulcsszóval kezdődik.",
        _ => string.Empty
    };
}
