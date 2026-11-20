namespace FinanceTracker.Application.Features.Analytics.GetOverview;

/// <summary>UC-21: az időszak mindkét napja beletartozik; megadás nélkül az aktuális hónap.</summary>
public sealed record GetOverviewQuery(DateOnly? From, DateOnly? To);
