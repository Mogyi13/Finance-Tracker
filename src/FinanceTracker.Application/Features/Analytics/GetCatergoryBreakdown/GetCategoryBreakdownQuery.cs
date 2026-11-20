namespace FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;

/// <summary>UC-22: az időszak mindkét napja beletartozik; megadás nélkül az aktuális hónap.</summary>
public sealed record GetCategoryBreakdownQuery(DateOnly? From, DateOnly? To);
