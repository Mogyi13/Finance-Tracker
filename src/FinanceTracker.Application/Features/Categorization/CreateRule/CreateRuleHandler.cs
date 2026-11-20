using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categorization.CreateRule;

/// <summary>UC-16: új kategorizálási szabály létrehozása.</summary>
public sealed class CreateRuleHandler(
    ICurrentUser currentUser,
    ICategorizationRuleRepository rules,
    ICategoryRepository categories,
    TimeProvider timeProvider)
{
    public async Task<Result<CategorizationRuleDto>> HandleAsync(CreateRuleCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var keyword = CategorizationRule.NormalizeKeyword(command.Keyword);

        var errors = new FieldErrors();
        CategorizationRuleFieldRules.Validate(keyword, command.MatchType, command.Priority, errors);

        var category = await categories.GetByIdAsync(command.CategoryId);
        CategorizationRuleFieldRules.ValidateCategory(category, userId, errors);

        if (!errors.Any && await rules.ExistsAsync(userId, keyword, command.MatchType, command.CategoryId))
        {
            errors.Add("keyword", "Már létezik azonos kulcsszavú, egyezési típusú és kategóriájú szabály.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var rule = CategorizationRule.Create(
            Guid.NewGuid(), userId, category!, keyword, command.MatchType, command.Priority, command.IsActive,
            timeProvider.GetUtcNow().UtcDateTime);
        await rules.AddAsync(rule);

        return Result<CategorizationRuleDto>.Ok(CategorizationRuleDto.From(rule, category!));
    }
}
