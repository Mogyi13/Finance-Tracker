namespace FinanceTracker.Domain.Enums;

/// <summary>A tranzakció kategorizálásának állapota.</summary>
public enum CategorizationStatus : byte
{
    Uncategorized = 0,
    RuleBased = 1,
    MachineLearning = 2,
    ManuallyAssigned = 3
}
