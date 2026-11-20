using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.UpdateRule;

/// <summary>UC-16: a szabály teljes módosítása.</summary>
public sealed record UpdateRuleCommand(
    string? Keyword,
    RuleMatchType MatchType,
    Guid CategoryId,
    int Priority,
    bool IsActive);
