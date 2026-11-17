using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Profile;

/// <summary>A felhasználó profiladatai (UC-04 válasz).</summary>
public sealed record UserProfileDto(Guid Id, string Email, string DisplayName, DateTime CreatedAt)
{
    internal static UserProfileDto From(User user)
        => new(user.Id, user.Email, user.DisplayName, user.CreatedAt);
}
