using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categories.CreateCategory;

/// <summary>UC-10: saját kategória létrehozása.</summary>
public sealed class CreateCategoryHandler(
    ICurrentUser currentUser,
    ICategoryRepository categories,
    TimeProvider timeProvider)
{
    public async Task<Result<CategoryDto>> HandleAsync(CreateCategoryCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var name = CategoryFieldRules.NormalizeName(command.Name);

        var errors = new FieldErrors();
        CategoryFieldRules.Validate(name, command.Type, errors);

        if (!errors.Any && await categories.NameExistsAsync(userId, name, command.Type))
        {
            errors.Add("name", "Már létezik ilyen nevű kategória ebben a típusban.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var category = Category.CreateCustom(Guid.NewGuid(), userId, name, command.Type, timeProvider.GetUtcNow().UtcDateTime);
        await categories.AddAsync(category);

        return Result<CategoryDto>.Ok(CategoryDto.From(category));
    }
}