using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.CreateRule;
using FinanceTracker.Application.Features.Categorization.DeleteRule;
using FinanceTracker.Application.Features.Categorization.GetRules;
using FinanceTracker.Application.Features.Categorization.SetRuleActive;
using FinanceTracker.Application.Features.Categorization.SetRulePriority;
using FinanceTracker.Application.Features.Categorization.UpdateRule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>UC-16: a felhasználó kategorizálási szabályainak kezelése. A szabályok kiértékelése (UC-15) az importnál és a tranzakció rögzítésekor történik.</summary>
[ApiController]
[Authorize]
[Route("api/categorization/rules")]
public class CategorizationRulesController(
    GetRulesHandler getRules,
    CreateRuleHandler create,
    UpdateRuleHandler update,
    SetRuleActiveHandler setActive,
    SetRulePriorityHandler setPriority,
    DeleteRuleHandler delete) : ControllerBase
{
    /// <summary>A saját szabályok kiértékelési sorrendben (a legmagasabb prioritású elöl).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategorizationRuleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await getRules.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType<CategorizationRuleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateRuleCommand command)
    {
        var result = await create.HandleAsync(command);
        return result.IsSuccess
            ? Created($"api/categorization/rules/{result.Value.Id}", result.Value)
            : this.ToProblem(result.Error!);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategorizationRuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateRuleCommand command)
    {
        var result = await update.HandleAsync(id, command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>A szabály aktiválása vagy deaktiválása.</summary>
    [HttpPatch("{id:guid}/active")]
    [ProducesResponseType<CategorizationRuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(Guid id, SetRuleActiveCommand command)
    {
        var result = await setActive.HandleAsync(id, command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>A szabály prioritásának módosítása. A nagyobb szám előbb értékelődik ki.</summary>
    [HttpPatch("{id:guid}/priority")]
    [ProducesResponseType<CategorizationRuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPriority(Guid id, SetRulePriorityCommand command)
    {
        var result = await setPriority.HandleAsync(id, command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await delete.HandleAsync(id);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
