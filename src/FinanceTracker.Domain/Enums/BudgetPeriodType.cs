namespace FinanceTracker.Domain.Enums;

/// <summary>UC-26: a költségkeret időszakának típusa (az adatbázisban tinyint, értékei a DATABASE.MD szerint).</summary>
public enum BudgetPeriodType : byte
{
    /// <summary>Naptári hónaponként ismétlődő keret.</summary>
    Monthly = 1,

    /// <summary>Hetenként (hétfőtől vasárnapig) ismétlődő keret.</summary>
    Weekly = 2,

    /// <summary>Egyetlen, kezdő és záró dátummal megadott időszak.</summary>
    Custom = 3
}
