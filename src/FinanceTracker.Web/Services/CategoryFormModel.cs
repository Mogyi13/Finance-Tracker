using System.ComponentModel.DataAnnotations;
using FinanceTracker.Application.Features.Categories;
using FinanceTracker.Application.Features.Categories.CreateCategory;
using FinanceTracker.Application.Features.Categories.UpdateCategory;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>A kategóriaűrlap modellje, kliensoldali validációval. A végső ellenőrzést a backend végzi.</summary>
public sealed class CategoryFormModel
{
    [Required(ErrorMessage = "A kategória nevének megadása kötelező.")]
    [StringLength(Category.MaxNameLength, ErrorMessage = "A kategória neve legfeljebb 100 karakter lehet.")]
    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; } = CategoryType.Expense;

    public static CategoryFormModel From(CategoryDto dto) => new() { Name = dto.Name, Type = dto.Type };

    public CreateCategoryCommand ToCreateCommand() => new(Name.Trim(), Type);

    public UpdateCategoryCommand ToUpdateCommand() => new(Name.Trim(), Type);
}
