namespace FinanceTracker.Domain.Enums;

/// <summary>UC-24: az ismétlődés gyakorisága (az adatbázisban tinyint, értékei a DATABASE.MD szerint).</summary>
public enum RecurrenceFrequency : byte
{
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    Yearly = 4
}
