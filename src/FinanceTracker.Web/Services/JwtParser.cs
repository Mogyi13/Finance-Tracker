using System.Security.Claims;
using System.Text.Json;

namespace FinanceTracker.Web.Services;

/// <summary>
/// A JWT payload kiolvasása a felhasználói felülethez. Az aláírást NEM ellenőrzi:
/// a token érvényességét kizárólag az API dönti el.
/// </summary>
public static class JwtParser
{
    public static ClaimsPrincipal ToPrincipal(string jwt)
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            return anonymous;
        }

        try
        {
            var base64 = parts[1].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

            using var doc = JsonDocument.Parse(Convert.FromBase64String(base64));
            var root = doc.RootElement;

            var claims = new List<Claim>();
            AddClaim(claims, root, "sub", ClaimTypes.NameIdentifier);
            AddClaim(claims, root, "email", ClaimTypes.Email);
            AddClaim(claims, root, "name", ClaimTypes.Name);

            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                return anonymous;
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return anonymous;
        }
    }

    private static void AddClaim(List<Claim> claims, JsonElement root, string jsonName, string claimType)
    {
        if (root.TryGetProperty(jsonName, out var element)
            && element.ValueKind == JsonValueKind.String
            && element.GetString() is { Length: > 0 } value)
        {
            claims.Add(new Claim(claimType, value));
        }
    }
}
