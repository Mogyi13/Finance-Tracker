using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Categories;
using FinanceTracker.Application.Features.Categories.CreateCategory;
using FinanceTracker.Application.Features.Categories.DeleteCategory;
using FinanceTracker.Application.Features.Categories.GetCategories;
using FinanceTracker.Application.Features.Categories.UpdateCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public class CategoriesController(
    GetCategoriesHandler getCategories,
    CreateCategoryHandler create,
    UpdateCategoryHandler update,
    DeleteCategoryHandler delete) : ControllerBase
{
    /// <summary>A felhasználó számára elérhető kategóriák: rendszerkategóriák és saját kategóriák.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await getCategories.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-10: új saját kategória.</summary>
    [HttpPost]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateCategoryCommand command)
    {
        var result = await create.HandleAsync(command);
        return result.IsSuccess
            ? Created($"api/categories/{result.Value.Id}", result.Value)
            : this.ToProblem(result.Error!);
    }

    /// <summary>UC-10: saját kategória módosítása. A rendszerkategória nem módosítható.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryCommand command)
    {
        var result = await update.HandleAsync(id, command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-10: saját kategória törlése. Használatban lévő vagy rendszerkategória nem törölhető.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await delete.HandleAsync(id);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
