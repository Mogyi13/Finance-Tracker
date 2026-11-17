using FinanceTracker.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Infrastructure;

public static class ResultExtensions
{
    /// <summary>Use case hiba leképezése egységes ProblemDetails válaszra.</summary>
    public static IActionResult ToProblem(this ControllerBase controller, Error error)
    {
        switch (error.Kind)
        {
            case ErrorKind.Validation:
                var fields = error.Fields is null
                    ? new Dictionary<string, string[]>()
                    : error.Fields.ToDictionary(kv => kv.Key, kv => kv.Value);
                return controller.ValidationProblem(new ValidationProblemDetails(fields)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = error.Message
                });
            case ErrorKind.Unauthorized:
                return controller.Problem(detail: error.Message, statusCode: StatusCodes.Status401Unauthorized, title: "Hitelesítési hiba");
            case ErrorKind.Locked:
                return controller.Problem(detail: error.Message, statusCode: StatusCodes.Status423Locked, title: "A fiók zárolva van");
            case ErrorKind.NotFound:
                return controller.Problem(detail: error.Message, statusCode: StatusCodes.Status404NotFound, title: "Nem található");
            default:
                return controller.Problem(detail: error.Message, statusCode: StatusCodes.Status500InternalServerError, title: "Szerverhiba");
        }
    }
}
