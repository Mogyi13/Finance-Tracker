namespace FinanceTracker.Domain.Enums;

/// <summary>A kategorizálási szabály egyezési típusa. Az értékek az adatbázisban tinyintként tárolódnak.</summary>
public enum RuleMatchType : byte
{
    /// <summary>A normalizált leírás pontosan megegyezik a kulcsszóval.</summary>
    Exact = 1,

    /// <summary>A normalizált leírás tartalmazza a kulcsszót.</summary>
    Contains = 2,

    /// <summary>A normalizált leírás a kulcsszóval kezdődik.</summary>
    StartsWith = 3
}
