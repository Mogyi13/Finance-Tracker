using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.API.Infrastructure;

/// <summary>A hitelesített felhasználó azonosítója a JWT "sub" claimjéből.</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
