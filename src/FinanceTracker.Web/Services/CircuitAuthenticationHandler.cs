using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FinanceTracker.Web.Services;

/// <summary>
/// A Web szerver HTTP szintjén nincs sütis hitelesítés (a JWT a Blazor-körben, a ProtectedSessionStorage-ban él),
/// ezért ez a séma csak átengedi a kérést. A tényleges védelmet az AuthorizeRouteView
/// (a JwtAuthenticationStateProvider alapján) és az API JWT-ellenőrzése adja.
/// </summary>
public sealed class CircuitAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Scheme = "Circuit";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(Scheme));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme)));
    }
}