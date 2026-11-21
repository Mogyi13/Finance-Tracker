using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Forecasting;
using FinanceTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>UC-28: havi kiadás és nettó pénzforgalom előrejelzése. Az eredmény becslés, nem garantált érték.</summary>
[ApiController]
[Authorize]
[Route("api/forecasting")]
public class ForecastingController(
    GenerateForecastHandler generate,
    GetForecastHandler get,
    EvaluateForecastHandler evaluate) : ControllerBase
{
    /// <summary>A mentett havi kiadás-előrejelzés az előzményekkel (mozgóátlag és lineáris regresszió).</summary>
    [HttpGet("monthly")]
    [ProducesResponseType<ForecastDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Monthly()
    {
        var result = await get.HandleAsync(ForecastType.MonthlyExpenses);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>A mentett havi nettó pénzforgalom-előrejelzés az előzményekkel.</summary>
    [HttpGet("cashflow")]
    [ProducesResponseType<ForecastDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CashFlow()
    {
        var result = await get.HandleAsync(ForecastType.MonthlyNetCashFlow);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>Az előrejelzés újraszámolása és mentése (a korábbi lecserélődik). Kevés előzményadatnál nem készül becslés.</summary>
    [HttpPost("generate")]
    [ProducesResponseType<ForecastDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate([FromQuery] ForecastType type = ForecastType.MonthlyExpenses)
    {
        var result = await generate.HandleAsync(type);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>A két módszer pontosságának összehasonlítása visszamenőleges teszttel (MAE, RMSE, MAPE).</summary>
    [HttpGet("evaluation")]
    [ProducesResponseType<ForecastEvaluationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Evaluation([FromQuery] ForecastType type = ForecastType.MonthlyExpenses)
    {
        var result = await evaluate.HandleAsync(type);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}
