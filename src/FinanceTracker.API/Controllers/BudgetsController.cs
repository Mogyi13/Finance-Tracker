using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Budgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>UC-26, UC-27: költségkeretek kezelése és a felhasználtságuk követése.</summary>
[ApiController]
[Authorize]
[Route("api/budgets")]
public class BudgetsController(
    GetBudgetsHandler getAll,
    CreateBudgetHandler create,
    UpdateBudgetHandler update,
    DeleteBudgetHandler delete,
    GetBudgetStatusHandler status) : ControllerBase
{
    /// <summary>A felhasználó költségkeretei.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BudgetDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await getAll.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-27: a keretek felhasználtsága az aktuális (vagy a megadott napot tartalmazó) időszakra, figyelmeztetési szinttel.</summary>
    [HttpGet("status")]
    [ProducesResponseType<BudgetStatusesDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Status([FromQuery] DateOnly? asOf = null)
    {
        var result = await status.HandleAsync(asOf);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-26: új költségkeret kiadási kategóriára.</summary>
    [HttpPost]
    [ProducesResponseType<BudgetDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(SaveBudgetCommand command)
    {
        var result = await create.HandleAsync(command);
        return result.IsSuccess
            ? Created($"api/budgets/{result.Value.Id}", result.Value)
            : this.ToProblem(result.Error!);
    }

    /// <summary>UC-26: költségkeret módosítása.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<BudgetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, SaveBudgetCommand command)
    {
        var result = await update.HandleAsync(id, command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-26: költségkeret törlése (a tranzakciókat nem érinti).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await delete.HandleAsync(id);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
