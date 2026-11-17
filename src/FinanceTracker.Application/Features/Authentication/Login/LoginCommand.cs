namespace FinanceTracker.Application.Features.Authentication.Login;

/// <summary>UC-02: bejelentkezés.</summary>
public sealed record LoginCommand(string Email, string Password);
