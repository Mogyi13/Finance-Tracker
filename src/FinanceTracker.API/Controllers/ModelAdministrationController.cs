using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Categorization.Training;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>
/// UC-20: a kategorizáló modell tanítása, kiértékelése és aktiválása. Csak a konfigurációban megnevezett
/// üzemeltetők érik el (<see cref="AuthorizationPolicies.ModelAdministrator"/>); a válaszok személyes adatot nem tartalmaznak.
/// Az új modell sosem aktiválódik automatikusan: a tanítás után az aktiválás külön, tudatos lépés.
/// </summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.ModelAdministrator)]
[Route("api/categorization/models")]
public class ModelAdministrationController(
    TrainModelHandler train,
    GetModelVersionsHandler list,
    GetModelVersionHandler get,
    ActivateModelHandler activate,
    DeactivateModelHandler deactivate) : ControllerBase
{
    /// <summary>Új modell tanítása a kézzel kategorizált tranzakciókból, kiértékelése a teszthalmazon. Az eredmény inaktív verzió.</summary>
    [HttpPost("train")]
    [ProducesResponseType<ModelVersionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Train(CancellationToken cancellationToken)
    {
        var result = await train.HandleAsync(cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : this.ToProblem(result.Error!);
    }

    /// <summary>Üzemeltetői jogosultság ellenőrzése (a felület ezzel dönti el, megjeleníti-e a modellkezelést). 204, ha van jogosultság; egyébként 403.</summary>
    [HttpGet("access")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Access() => NoContent();

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ModelVersionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await list.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>Egy verzió részletei a teljes kiértékelési jelentéssel.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ModelVersionDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await get.HandleAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>Verzió aktiválása; csak az aktiválási feltételeknek megfelelő verzió aktiválható.</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType<ModelVersionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await activate.HandleAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>Az aktív modell kivezetése: ezután csak a szabályok kategorizálnak automatikusan.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType<ModelVersionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await deactivate.HandleAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}
