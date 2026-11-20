using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization;

public sealed record CategorizationRuleDto(
    Guid Id,
    string Keyword,
    RuleMatchType MatchType,
    Guid CategoryId,
    string CategoryName,
    CategoryType CategoryType,
    int Priority,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    internal static CategorizationRuleDto From(CategorizationRule rule, Category category)
        => new(
            rule.Id,
            rule.Keyword,
            rule.MatchType,
            rule.CategoryId,
            category.Name,
            category.Type,
            rule.Priority,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt);
}
