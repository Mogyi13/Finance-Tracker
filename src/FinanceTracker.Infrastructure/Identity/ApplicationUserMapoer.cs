using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Infrastructure.Identity;

/// <summary>A domain User és az Identity perzisztencia-modell (ApplicationUser) közti leképezés.</summary>
internal static class ApplicationUserMapper
{
    public static User ToDomain(this ApplicationUser entity)
        => User.Restore(
            entity.Id,
            entity.Email ?? string.Empty,
            entity.DisplayName,
            // Az adatbázis datetime2 értéke Kind nélkül érkezik; a tárolt idő mindig UTC.
            DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc));

    public static ApplicationUser ToEntity(this User user)
        => new()
        {
            Id = user.Id,
            UserName = user.Email,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };
}
