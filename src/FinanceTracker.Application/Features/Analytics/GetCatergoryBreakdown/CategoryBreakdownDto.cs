namespace FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;

/// <summary>UC-22: egy kategória kiadásai; a <see cref="Share"/> az adott pénznem összes kiadásán belüli arány (0 és 1 között).</summary>
public sealed record CategoryExpenseDto(
    Guid? CategoryId,
    string CategoryName,
    decimal Amount,
    decimal Share,
    int TransactionCount);

/// <summary>Egy pénznem kategóriánkénti bontása. Csak azonos pénznemű összegek vannak összesítve (BR-09).</summary>
public sealed record CurrencyCategoryBreakdownDto(
    string Currency,
    decimal TotalExpense,
    int TransactionCount,
    IReadOnlyList<CategoryExpenseDto> Categories);

public sealed record CategoryBreakdownDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<CurrencyCategoryBreakdownDto> Currencies);
