using FinanceTracker.Application.Common;

namespace FinanceTracker.Application.Features.Authentication.Login;

internal static class LoginValidator
{
    public static FieldErrors Validate(LoginCommand command)
    {
        var errors = new FieldErrors();

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors.Add("email", "Az e-mail-cím megadása kötelező.");
        }

        if (string.IsNullOrEmpty(command.Password))
        {
            errors.Add("password", "A jelszó megadása kötelező.");
        }

        return errors;
    }
}
