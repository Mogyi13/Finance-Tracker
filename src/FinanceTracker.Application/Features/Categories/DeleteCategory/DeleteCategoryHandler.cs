using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categories.DeleteCategory;

/// <summary>
/// UC-10: saját kategória törlése. A használatban lévő kategória nem törölhető
/// (újrahozzárendelési folyamat egyelőre nincs).
/// </summary>
public sealed class DeleteCategoryHandler(ICurrentUser currentUser, ICategoryRepository categories)
{
    public async Task<Result<Unit>> HandleAsync(Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var category = await categories.GetByIdAsync(id);
        if (category is null || !category.IsAvailableTo(userId))
        {
            return Error.NotFound("A kategória nem található.");
        }

        if (category.IsSystemCategory)
        {
            return Error.Forbidden("A rendszerkategória nem törölhető.");
        }

        if (await categories.IsInUseAsync(category.Id))
        {
            return Error.Conflict("A kategória nem törölhető, mert tranzakciók hivatkoznak rá.");
        }

        await categories.DeleteAsync(category);
        return Result<Unit>.Ok(Unit.Value);
    }
}
