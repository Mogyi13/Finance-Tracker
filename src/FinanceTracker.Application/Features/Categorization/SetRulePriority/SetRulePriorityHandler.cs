using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categorization.SetRulePriority;

/// <summary>UC-16: a szabály prioritásának módosítása.</summary>
public sealed class SetRulePriorityHandler(
    ICurrentUser currentUser,
    ICategorizationRuleRepository rules,
    ICategoryRepository categories,
    TimeProvider timeProvider)
{
    public async Task<Result<CategorizationRuleDto>> HandleAsync(Guid id, SetRulePriorityCommand command)
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

        var errors = new FieldErrors();
        CategorizationRuleFieldRules.ValidatePriority(command.Priority, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        var category = await categories.GetByIdAsync(rule.CategoryId);
        if (category is null)
        {
            return Error.NotFound("A szabály kategóriája nem található.");
        }

        rule.SetPriority(command.Priority, timeProvider.GetUtcNow().UtcDateTime);
        await rules.UpdateAsync(rule);

        return Result<CategorizationRuleDto>.Ok(CategorizationRuleDto.From(rule, category));
    }
}
