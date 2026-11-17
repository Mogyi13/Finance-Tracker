using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Common.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>A felhasználó módosított adatainak mentése.</summary>
    Task<Result<User>> UpdateAsync(User user);
}
