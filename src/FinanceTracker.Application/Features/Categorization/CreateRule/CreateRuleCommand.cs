using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.CreateRule;

/// <summary>UC-16: új szabály. A prioritás és az aktív állapot elhagyható.</summary>
public sealed record CreateRuleCommand(
    string? Keyword,
    RuleMatchType MatchType,
    Guid CategoryId,
    int Priority = CategorizationRule.DefaultPriority,
    bool IsActive = true);
