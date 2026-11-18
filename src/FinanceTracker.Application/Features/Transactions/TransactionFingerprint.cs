using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Transactions;

/// <summary>
/// Determinisztikus tranzakciós ujjlenyomat (SHA-256, hexadecimálisan) a duplikációfelismeréshez.
/// A dátum, típus, összeg, pénznem és a normalizált leírás alapján készül; a felhasználó szerinti
/// szűkítést a hívó végzi. Gyakorlati heurisztika, nem bizonyíték az azonosságra.
/// </summary>
public static class TransactionFingerprint
{
    public static string Compute(DateOnly date, TransactionType type, decimal amount, string currency, string? description)
    {
        var canonical = string.Join(
            '|',
            date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ((byte)type).ToString(CultureInfo.InvariantCulture),
            amount.ToString("0.00", CultureInfo.InvariantCulture),
            currency.Trim().ToUpperInvariant(),
            NormalizeDescription(description));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    /// <summary>Szóközök összevonása és kisbetűsítés, hogy a lényegtelen eltérések ne számítsanak.</summary>
    public static string NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var parts = description.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts).ToLowerInvariant();
    }
}
