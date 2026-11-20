using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categorization.UpdateRule;

/// <summary>UC-16: kategorizálási szabály szerkesztése.</summary>
public sealed class UpdateRuleHandler(
    ICurrentUser currentUser,
    ICategorizationRuleRepository rules,
    ICategoryRepository categories,
    TimeProvider timeProvider)
{
    public async Task<Result<CategorizationRuleDto>> HandleAsync(Guid id, UpdateRuleCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        // Más felhasználó szabálya ugyanúgy „nem található”, mint a nem létező.
        var rule = await rules.GetByIdAsync(userId, id);
        if (rule is null)
        {
            return Error.NotFound("A szabály nem található.");
        }

        var keyword = CategorizationRule.NormalizeKeyword(command.Keyword);

        var errors = new FieldErrors();
        CategorizationRuleFieldRules.Validate(keyword, command.MatchType, command.Priority, errors);

        var category = await categories.GetByIdAsync(command.CategoryId);
        CategorizationRuleFieldRules.ValidateCategory(category, userId, errors);

        if (!errors.Any && await rules.ExistsAsync(userId, keyword, command.MatchType, command.CategoryId, excludeRuleId: rule.Id))
        {
            errors.Add("keyword", "Már létezik azonos kulcsszavú, egyezési típusú és kategóriájú szabály.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        rule.Update(category!, keyword, command.MatchType, command.Priority, command.IsActive, timeProvider.GetUtcNow().UtcDateTime);
        await rules.UpdateAsync(rule);

        return Result<CategorizationRuleDto>.Ok(CategorizationRuleDto.From(rule, category!));
    }
}
