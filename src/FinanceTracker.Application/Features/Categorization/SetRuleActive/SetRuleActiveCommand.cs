namespace FinanceTracker.Application.Features.Categorization.SetRuleActive;

/// <summary>UC-16: a szabály aktiválása vagy deaktiválása.</summary>
public sealed record SetRuleActiveCommand(bool IsActive);
