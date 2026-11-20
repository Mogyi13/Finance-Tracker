using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Categories.UpdateCategory;

/// <summary>UC-10: saját kategória módosítása.</summary>
public sealed class UpdateCategoryHandler(ICurrentUser currentUser, ICategoryRepository categories)
{
    public async Task<Result<CategoryDto>> HandleAsync(Guid id, UpdateCategoryCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var category = await categories.GetByIdAsync(id);

        // Más felhasználó kategóriája ugyanúgy "nem található", mint a nem létező.
        if (category is null || !category.IsAvailableTo(userId))
        {
            return Error.NotFound("A kategória nem található.");
        }

        if (category.IsSystemCategory)
        {
            return Error.Forbidden("A rendszerkategória nem módosítható.");
        }

        var name = CategoryFieldRules.NormalizeName(command.Name);

        var errors = new FieldErrors();
        CategoryFieldRules.Validate(name, command.Type, errors);

        if (!errors.Any)
        {
            if (await categories.NameExistsAsync(userId, name, command.Type, excludeCategoryId: category.Id))
            {
                errors.Add("name", "Már létezik ilyen nevű kategória ebben a típusban.");
            }

            // A típus csak akkor változhat, ha ez nem teszi érvénytelenné a már hozzárendelt tranzakciókat.
            if (command.Type != category.Type && await categories.IsInUseAsync(category.Id))
            {
                errors.Add("type", "A kategória típusa nem módosítható, mert tranzakciók vagy kategorizálási szabályok hivatkoznak rá.");
            }
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        category.Update(name, command.Type);
        await categories.UpdateAsync(category);

        return Result<CategoryDto>.Ok(CategoryDto.From(category));
    }
}
