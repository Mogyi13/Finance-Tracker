using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> users,
    ILogger<IdentityService> logger) : IIdentityService
{
    private const string InvalidCredentials = "Hibás e-mail-cím vagy jelszó.";

    public async Task<Result<User>> RegisterAsync(User user, string password)
    {
        var result = await users.CreateAsync(user.ToEntity(), password);
        if (!result.Succeeded)
        {
            return IdentityErrorMapper.ToError(result.Errors);
        }

        logger.LogInformation("Új felhasználó regisztrált: {UserId}", user.Id);
        return Result<User>.Ok(user);
    }

    public async Task<Result<User>> VerifyCredentialsAsync(string email, string password)
    {
        var entity = await users.FindByEmailAsync(email);
        if (entity is null)
        {
            return Error.Unauthorized(InvalidCredentials);
        }

        if (await users.IsLockedOutAsync(entity))
        {
            logger.LogWarning("Zárolt fiók bejelentkezési kísérlete: {UserId}", entity.Id);
            return Error.Locked("A fiók túl sok sikertelen próbálkozás miatt átmenetileg zárolva van. Próbáld újra később.");
        }

        if (!await users.CheckPasswordAsync(entity, password))
        {
            await users.AccessFailedAsync(entity);
            logger.LogInformation("Sikertelen bejelentkezés: {UserId}", entity.Id);
            return Error.Unauthorized(InvalidCredentials);
        }

        await users.ResetAccessFailedCountAsync(entity);
        return Result<User>.Ok(entity.ToDomain());
    }
}
