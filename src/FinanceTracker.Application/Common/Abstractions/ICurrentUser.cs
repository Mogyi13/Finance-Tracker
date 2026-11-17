namespace FinanceTracker.Application.Common.Abstractions;

/// <summary>A hitelesített felhasználó azonosítója a kérés kontextusából.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
}
