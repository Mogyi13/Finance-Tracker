using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Common.Abstractions;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessToken CreateToken(User user);
}
