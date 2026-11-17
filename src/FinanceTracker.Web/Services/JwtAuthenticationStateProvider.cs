using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace FinanceTracker.Web.Services;

public sealed record StoredToken(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// Blazor Server hitelesítési állapot: a JWT a böngésző sessionStorage-ában van,
/// a Data Protection által titkosítva (ProtectedSessionStorage).
/// Csak interaktív, előrenderelés nélküli komponensekből használható.
/// </summary>
public sealed class JwtAuthenticationStateProvider(ProtectedSessionStorage storage) : AuthenticationStateProvider
{
    private const string StorageKey = "financetracker.auth";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private StoredToken? _token;
    private bool _loaded;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetValidTokenAsync();
        return token is null
            ? Anonymous
            : new AuthenticationState(JwtParser.ToPrincipal(token.AccessToken));
    }

    public async Task<string?> GetAccessTokenAsync() => (await GetValidTokenAsync())?.AccessToken;

    public async Task SignInAsync(string accessToken, DateTimeOffset expiresAt)
    {
        _token = new StoredToken(accessToken, expiresAt);
        _loaded = true;
        await storage.SetAsync(StorageKey, _token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>UC-03: a hitelesítési állapot és a token törlése.</summary>
    public async Task SignOutAsync()
    {
        _token = null;
        _loaded = true;
        await storage.DeleteAsync(StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private async Task<StoredToken?> GetValidTokenAsync()
    {
        if (!_loaded)
        {
            try
            {
                var result = await storage.GetAsync<StoredToken>(StorageKey);
                _token = result.Success ? result.Value : null;
                _loaded = true;
            }
            catch (InvalidOperationException)
            {
                // A JS interop még nem érhető el (például előrenderelés alatt).
                return null;
            }
            catch (JSDisconnectedException)
            {
                return null;
            }
        }

        if (_token is not null && _token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _token = null;
            try
            {
                await storage.DeleteAsync(StorageKey);
            }
            catch (Exception ex) when (ex is InvalidOperationException or JSDisconnectedException)
            {
                // A lejárt token törlése best-effort.
            }
        }

        return _token;
    }
}
