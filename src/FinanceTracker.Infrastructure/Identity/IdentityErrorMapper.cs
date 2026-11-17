using FinanceTracker.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.Infrastructure.Identity;

/// <summary>Az Identity hibáinak leképezése mezőnkénti, magyar nyelvű validációs hibákra.</summary>
internal static class IdentityErrorMapper
{
    public static Error ToError(IEnumerable<IdentityError> identityErrors)
    {
        var errors = new FieldErrors();
        foreach (var error in identityErrors)
        {
            var (field, message) = Translate(error);
            errors.Add(field, message);
        }

        return errors.ToError();
    }

    private static (string Field, string Message) Translate(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => ("email", "Ezzel az e-mail-címmel már létezik fiók."),
        "InvalidEmail" or "InvalidUserName" => ("email", "Az e-mail-cím formátuma érvénytelen."),
        "PasswordTooShort" => ("password", "A jelszó legalább 8 karakter hosszú legyen."),
        "PasswordRequiresDigit" => ("password", "A jelszó tartalmazzon legalább egy számjegyet."),
        "PasswordRequiresLower" => ("password", "A jelszó tartalmazzon legalább egy kisbetűt."),
        "PasswordRequiresUpper" => ("password", "A jelszó tartalmazzon legalább egy nagybetűt."),
        "PasswordRequiresNonAlphanumeric" => ("password", "A jelszó tartalmazzon legalább egy speciális karaktert."),
        "PasswordRequiresUniqueChars" => ("password", "A jelszó több különböző karaktert tartalmazzon."),
        _ => ("general", error.Description)
    };
}
