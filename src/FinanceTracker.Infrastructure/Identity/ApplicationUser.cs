using Microsoft.AspNetCore.Identity;

namespace FinanceTracker.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
