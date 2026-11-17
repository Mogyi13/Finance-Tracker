using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Profile.UpdateProfile;

namespace FinanceTracker.Application.Features.Profile.UpdateProfile;

/// <summary>UC-04: profil módosítása.</summary>
public sealed class UpdateProfileHandler(ICurrentUser currentUser, IUserRepository users)
{
    public async Task<Result<UserProfileDto>> HandleAsync(UpdateProfileCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var normalized = command with { DisplayName = (command.DisplayName ?? string.Empty).Trim() };

        var errors = UpdateProfileValidator.Validate(normalized);
        if (errors.Any)
        {
            return errors.ToError();
        }

        var user = await users.GetByIdAsync(userId);
        if (user is null)
        {
            return Error.Unauthorized("A felhasználó nem található.");
        }

        user.ChangeDisplayName(normalized.DisplayName);

        var saved = await users.UpdateAsync(user);
        if (!saved.IsSuccess)
        {
            return saved.Error!;
        }

        return Result<UserProfileDto>.Ok(UserProfileDto.From(saved.Value));
    }
}
