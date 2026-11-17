using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Profile.GetProfile;

/// <summary>UC-04: a bejelentkezett felhasználó profiljának lekérése.</summary>
public sealed class GetProfileHandler(ICurrentUser currentUser, IUserRepository users)
{
    public async Task<Result<UserProfileDto>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var user = await users.GetByIdAsync(userId);
        if (user is null)
        {
            return Error.Unauthorized("A felhasználó nem található.");
        }

        return Result<UserProfileDto>.Ok(UserProfileDto.From(user));
    }
}
