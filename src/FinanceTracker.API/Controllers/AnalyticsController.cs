using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Analytics.ComparePeriods;
using FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;
using FinanceTracker.Application.Features.Analytics.GetOverview;
using FinanceTracker.Application.Features.Analytics.Recurring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>UC-21..UC-24: pénzügyi áttekintés, kategóriánkénti elemzés, időszakok összehasonlítása és ismétlődő kiadások.</summary>
[ApiController]
[Authorize]
[Route("api/analytics")]
public class AnalyticsController(
    GetOverviewHandler overview,
    GetCategoryBreakdownHandler categories,
    ComparePeriodsHandler compare,
    GetRecurringHandler recurring,
    DetectRecurringHandler detectRecurring) : ControllerBase
{
    /// <summary>
    /// UC-21: összes bevétel, kiadás, nettó pénzforgalom és tranzakciószám pénznemenként a megadott időszakra
    /// (megadás nélkül az aktuális hónapra). A nettó pénzforgalom nem a bankszámla egyenlege.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType<FinancialOverviewDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Overview([FromQuery] GetOverviewQuery query)
    {
        var result = await overview.HandleAsync(query);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-22: a kiadások kategóriánkénti bontása (összeg, arány, tranzakciószám) pénznemenként.</summary>
    [HttpGet("categories")]
    [ProducesResponseType<CategoryBreakdownDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Categories([FromQuery] GetCategoryBreakdownQuery query)
    {
        var result = await categories.HandleAsync(query);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-23: két időszak összehasonlítása (bevétel, kiadás, nettó pénzforgalom, kategóriánkénti kiadás; abszolút és százalékos eltérés).</summary>
    [HttpGet("compare")]
    [ProducesResponseType<PeriodComparisonDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Compare([FromQuery] ComparePeriodsQuery query)
    {
        var result = await compare.HandleAsync(query);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-24: a legutóbb felismert, valószínűleg ismétlődő kiadások (becslés, nem megerősített előfizetés).</summary>
    [HttpGet("recurring")]
    [ProducesResponseType<RecurringTransactionsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Recurring()
    {
        var result = await recurring.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-24: az ismétlődő kiadások újraelemzése a korábbi tranzakciókból; az eredmény lecseréli a korábbit.</summary>
    [HttpPost("recurring/analyze")]
    [ProducesResponseType<RecurringTransactionsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AnalyzeRecurring()
    {
        var result = await detectRecurring.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}
