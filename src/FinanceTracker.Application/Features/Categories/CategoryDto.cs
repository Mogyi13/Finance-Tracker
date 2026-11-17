using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categories;

public sealed record CategoryDto(Guid Id, string Name, CategoryType Type, bool IsSystemCategory)
{
    internal static CategoryDto From(Category category)
        => new(category.Id, category.Name, category.Type, category.IsSystemCategory);
}
