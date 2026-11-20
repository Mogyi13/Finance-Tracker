namespace FinanceTracker.Domain.Enums;

/// <summary>Egy CSV-import feldolgozásának állapota.</summary>
public enum ImportBatchStatus : byte
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    PartiallyCompleted = 4,
    Failed = 5
}
