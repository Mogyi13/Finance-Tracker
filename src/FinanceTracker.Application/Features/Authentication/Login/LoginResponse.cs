namespace FinanceTracker.Application.Features.Authentication.Login;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
