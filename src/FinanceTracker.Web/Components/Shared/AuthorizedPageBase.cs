using FinanceTracker.Web.Services;
using Microsoft.AspNetCore.Components;

namespace FinanceTracker.Web.Components.Shared;

/// <summary>
/// A bejelentkezést igénylő oldalak közös alapja: ha az API 401-et ad (lejárt vagy érvénytelen token),
/// kiléptet és a belépő oldalra visz.
/// </summary>
public abstract class AuthorizedPageBase : ComponentBase
{
    [Inject] protected JwtAuthenticationStateProvider Session { get; set; } = default!;

    [Inject] protected UserDisplayState UserState { get; set; } = default!;

    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    /// <summary>Igazzal tér vissza, ha a hívás 401 volt, és a felhasználót már átirányítottuk a belépéshez.</summary>
    protected async Task<bool> HandleUnauthorizedAsync<T>(ApiResult<T> result)
    {
        if (!result.IsUnauthorized)
        {
            return false;
        }

        UserState.Set(null);
        await Session.SignOutAsync();
        Navigation.NavigateTo("login");
        return true;
    }
}
