using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Validation;

namespace FinanceTracker.Application.Features.Authentication.Register;

internal static class RegisterValidator
{
    public const int MaxPasswordLength = 128;

    public static FieldErrors Validate(RegisterCommand command)
    {
        var errors = new FieldErrors();

        UserFieldRules.ValidateEmail(command.Email, errors);
        UserFieldRules.ValidateDisplayName(command.DisplayName, errors);

        if (string.IsNullOrEmpty(command.Password))
        {
            errors.Add("password", "A jelszó megadása kötelező.");
        }
        else if (command.Password.Length > MaxPasswordLength)
        {
            errors.Add("password", $"A jelszó legfeljebb {MaxPasswordLength} karakter lehet.");
        }

        // A jelszó erősségét az Identity szabályai ellenőrzik (IIdentityService).
        return errors;
    }
}
