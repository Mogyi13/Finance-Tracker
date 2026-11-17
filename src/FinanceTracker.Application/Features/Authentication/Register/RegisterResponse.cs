namespace FinanceTracker.Application.Features.Authentication.Register;

public sealed record RegisterResponse(Guid UserId, string Email, string DisplayName, DateTime CreatedAt);
