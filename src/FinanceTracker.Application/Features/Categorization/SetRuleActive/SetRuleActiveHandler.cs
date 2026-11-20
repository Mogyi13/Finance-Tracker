using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categorization.SetRuleActive;

/// <summary>UC-16: a szabály aktiválása vagy deaktiválása.</summary>
public sealed class SetRuleActiveHandler(
    ICurrentUser currentUser,
    ICategorizationRuleRepository rules,
    ICategoryRepository categories,
    TimeProvider timeProvider)
{
    public async Task<Result<CategorizationRuleDto>> HandleAsync(Guid id, SetRuleActiveCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var rule = await rules.GetByIdAsync(userId, id);
        if (rule is null)
        {
            return Error.NotFound("A szabály nem található.");
        }

        var category = await categories.GetByIdAsync(rule.CategoryId);
        if (category is null)
        {
            return Error.NotFound("A szabály kategóriája nem található.");
        }

        rule.SetActive(command.IsActive, timeProvider.GetUtcNow().UtcDateTime);
        await rules.UpdateAsync(rule);

        return Result<CategorizationRuleDto>.Ok(CategorizationRuleDto.From(rule, category));
    }
}
