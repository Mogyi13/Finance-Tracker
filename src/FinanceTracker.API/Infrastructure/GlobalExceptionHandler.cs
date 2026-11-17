using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Infrastructure;

/// <summary>Váratlan hibák egységes ProblemDetails válasza; a kivétel részleteit nem adja ki a kliensnek.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Kezeletlen hiba: {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Szerverhiba",
                Detail = "Váratlan hiba történt. Próbáld újra később."
            },
            cancellationToken);

        return true;
    }
}
