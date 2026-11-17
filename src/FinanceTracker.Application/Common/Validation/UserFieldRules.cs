using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Common.Validation;

/// <summary>A felhasználói mezők közös validációja. A szabályok forrása a Domain (User).</summary>
internal static class UserFieldRules
{
    public static void ValidateEmail(string email, FieldErrors errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("email", "Az e-mail-cím megadása kötelező.");
        }
        else if (!User.IsValidEmail(email))
        {
            errors.Add("email", "Az e-mail-cím formátuma érvénytelen.");
        }
    }

    public static void ValidateDisplayName(string displayName, FieldErrors errors)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors.Add("displayName", "A megjelenítendő név megadása kötelező.");
        }
        else if (!User.IsValidDisplayName(displayName))
        {
            errors.Add("displayName", $"A megjelenítendő név legfeljebb {User.MaxDisplayNameLength} karakter lehet.");
        }
    }
}
