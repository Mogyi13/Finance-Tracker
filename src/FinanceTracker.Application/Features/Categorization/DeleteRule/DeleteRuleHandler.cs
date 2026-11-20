using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categorization.DeleteRule;

/// <summary>
/// UC-16: szabály törlése. A már kategorizált tranzakciók kategóriája és állapota nem változik;
/// a szabály törlése csak a későbbi automatikus kategorizálást érinti.
/// </summary>
public sealed class DeleteRuleHandler(ICurrentUser currentUser, ICategorizationRuleRepository rules)
{
    public async Task<Result<Unit>> HandleAsync(Guid id)
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

        await rules.DeleteAsync(rule);
        return Result<Unit>.Ok(Unit.Value);
    }
}
