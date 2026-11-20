using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.CorrectCategory;
using FinanceTracker.Application.Features.Categorization.GetSuggestions;
using FinanceTracker.Application.Features.Categorization.PredictCategory;
using FinanceTracker.Application.Features.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>UC-17..UC-19: kézi kategóriajavítás, valamint a szabályok és az ML-modell alapú kategóriajavaslatok.</summary>
[ApiController]
[Authorize]
[Route("api/categorization")]
public class CategorizationController(
    CorrectCategoryHandler correct,
    PredictCategoryHandler predict,
    GetSuggestionsHandler suggestions) : ControllerBase
{
    /// <summary>
    /// UC-17: egy saját tranzakció kategóriájának javítása (vagy egy javaslat megerősítése). A tranzakció kézzel
    /// besorolt lesz, és a javítás visszajelzésként megmarad a későbbi modellértékeléshez. Újratanítást nem indít.
    /// </summary>
    [HttpPost("feedback")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Feedback(CorrectCategoryCommand command)
    {
        var result = await correct.HandleAsync(command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-18, UC-19: kategóriajavaslat egy tranzakcióleíráshoz (nem módosít semmit).</summary>
    [HttpPost("predict")]
    [ProducesResponseType<CategoryPredictionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Predict(PredictCategoryCommand command)
    {
        var result = await predict.HandleAsync(command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-19: a kategorizálatlan tranzakciókhoz tartozó, megerősítésre váró javaslatok.</summary>
    [HttpGet("suggestions")]
    [ProducesResponseType<SuggestionsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Suggestions()
    {
        var result = await suggestions.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}
