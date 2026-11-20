namespace FinanceTracker.Application.Features.Categorization.SetRulePriority;

/// <summary>UC-16: a szabály prioritásának módosítása. A nagyobb szám előbb értékelődik ki.</summary>
public sealed record SetRulePriorityCommand(int Priority);
