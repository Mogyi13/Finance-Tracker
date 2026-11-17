using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Authentication.Register;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Authentication.Register;

/// <summary>UC-01: Regisztráció.</summary>
public sealed class RegisterHandler(IIdentityService identity, TimeProvider timeProvider)
{
    public async Task<Result<RegisterResponse>> HandleAsync(RegisterCommand command)
    {
        var normalized = command with
        {
            Email = (command.Email ?? string.Empty).Trim(),
            DisplayName = (command.DisplayName ?? string.Empty).Trim(),
            Password = command.Password ?? string.Empty
        };

        var errors = RegisterValidator.Validate(normalized);
        if (errors.Any)
        {
            return errors.ToError();
        }

        var user = User.Create(
            Guid.NewGuid(),
            normalized.Email,
            normalized.DisplayName,
            timeProvider.GetUtcNow().UtcDateTime);

        var registered = await identity.RegisterAsync(user, normalized.Password);
        if (!registered.IsSuccess)
        {
            return registered.Error!;
        }

        var created = registered.Value;
        return Result<RegisterResponse>.Ok(
            new RegisterResponse(created.Id, created.Email, created.DisplayName, created.CreatedAt));
    }
}
