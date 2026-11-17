using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.Infrastructure.Identity;

public sealed class UserRepository(UserManager<ApplicationUser> users) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var entity = await users.FindByIdAsync(id.ToString());
        return entity?.ToDomain();
    }

    public async Task<Result<User>> UpdateAsync(User user)
    {
        var entity = await users.FindByIdAsync(user.Id.ToString());
        if (entity is null)
        {
            return Error.NotFound("A felhasználó nem található.");
        }

        entity.DisplayName = user.DisplayName;

        var result = await users.UpdateAsync(entity);
        if (!result.Succeeded)
        {
            return IdentityErrorMapper.ToError(result.Errors);
        }

        return Result<User>.Ok(entity.ToDomain());
    }
}
