using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categories.GetCategories;

/// <summary>A felhasználó számára elérhető kategóriák listája (a tranzakciós űrlap választójához).</summary>
public sealed class GetCategoriesHandler(ICurrentUser currentUser, ICategoryRepository categories)
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var available = await categories.GetAvailableAsync(userId);
        IReadOnlyList<CategoryDto> dtos = available.Select(CategoryDto.From).ToList();
        return Result<IReadOnlyList<CategoryDto>>.Ok(dtos);
    }
}
