namespace FinanceTracker.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>HS256 aláíró kulcs, legalább 32 bájt. Konfigurációból érkezik, a repóba nem kerül.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 60;
}
