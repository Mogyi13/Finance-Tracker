using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Anomalies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>UC-25: szokatlan kiadások (anomáliák) felismerése. Az eredmény statisztikai jelzés, nem hiba vagy csalás.</summary>
[ApiController]
[Authorize]
[Route("api/analytics/anomalies")]
public class AnomaliesController(
    GetAnomaliesHandler get,
    AnalyzeAnomaliesHandler analyze,
    ReviewAnomalyHandler review) : ControllerBase
{
    /// <summary>A mentett anomáliák (alapból csak az át nem tekintettek).</summary>
    [HttpGet]
    [ProducesResponseType<AnomaliesDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] bool includeReviewed = false)
    {
        var result = await get.HandleAsync(includeReviewed);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>
    /// A kiadások elemzése és az anomáliák mentése. A küszöb (z-pontszám, 1,5..6) megadás nélkül a konfigurált érték.
    /// Kevés előzményadatnál az eredmény ezt jelzi, és nem készül anomália.
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType<AnomalyAnalysisDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze([FromQuery] double? threshold = null)
    {
        var result = await analyze.HandleAsync(threshold);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>Az anomália áttekintettnek jelölése.</summary>
    [HttpPost("{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(Guid id)
    {
        var result = await review.HandleAsync(id);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
