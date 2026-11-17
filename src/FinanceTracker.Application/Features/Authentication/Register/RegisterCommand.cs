namespace FinanceTracker.Application.Features.Authentication.Register;

/// <summary>UC-01: új felhasználó regisztrációja.</summary>
public sealed record RegisterCommand(string Email, string DisplayName, string Password);
