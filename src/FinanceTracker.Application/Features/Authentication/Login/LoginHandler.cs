using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Authentication.Login;

namespace FinanceTracker.Application.Features.Authentication.Login;

/// <summary>UC-02: Bejelentkezés.</summary>
public sealed class LoginHandler(IIdentityService identity, ITokenService tokens)
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command)
    {
        var normalized = command with
        {
            Email = (command.Email ?? string.Empty).Trim(),
            Password = command.Password ?? string.Empty
        };

        var errors = LoginValidator.Validate(normalized);
        if (errors.Any)
        {
            return errors.ToError();
        }

        var verified = await identity.VerifyCredentialsAsync(normalized.Email, normalized.Password);
        if (!verified.IsSuccess)
        {
            return verified.Error!;
        }

        var token = tokens.CreateToken(verified.Value);
        return Result<LoginResponse>.Ok(new LoginResponse(token.Value, token.ExpiresAt));
    }
}
