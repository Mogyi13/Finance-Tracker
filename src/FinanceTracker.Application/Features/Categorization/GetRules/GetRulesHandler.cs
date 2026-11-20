using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categorization.GetRules;

/// <summary>UC-16: a bejelentkezett felhasználó szabályai kiértékelési sorrendben (a legmagasabb prioritású elöl).</summary>
public sealed class GetRulesHandler(
    ICurrentUser currentUser,
    ICategorizationRuleRepository rules,
    ICategoryRepository categories)
{
    public async Task<Result<IReadOnlyList<CategorizationRuleDto>>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var userRules = await rules.GetAllAsync(userId);
        var byId = (await categories.GetAvailableAsync(userId)).ToDictionary(c => c.Id);

        // A szabály kategóriája a törlési védelem miatt mindig létezik; a biztonság kedvéért a hiányzót kihagyjuk.
        IReadOnlyList<CategorizationRuleDto> items = userRules
            .Where(r => byId.ContainsKey(r.CategoryId))
            .Select(r => CategorizationRuleDto.From(r, byId[r.CategoryId]))
            .ToList();

        return Result<IReadOnlyList<CategorizationRuleDto>>.Ok(items);
    }
}
