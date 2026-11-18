using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Categories;
using FinanceTracker.Application.Features.Categories.GetCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public class CategoriesController(GetCategoriesHandler getCategories) : ControllerBase
{
    /// <summary>A felhasználó számára elérhető kategóriák: rendszerkategóriák és saját kategóriák.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await getCategories.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}
