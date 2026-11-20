using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Analytics.Recurring;

/// <summary>UC-24: egy felismert, valószínűleg ismétlődő kiadás. Becslés, nem megerősített előfizetés.</summary>
public sealed record RecurringTransactionDto(
    Guid Id,
    string Description,
    Guid? CategoryId,
    string? CategoryName,
    decimal ExpectedAmount,
    string Currency,
    RecurrenceFrequency Frequency,
    DateOnly LastOccurrence,
    DateOnly NextExpectedDate,
    decimal Confidence);

/// <summary>Az ismétlődő kiadások becsült havi terhelése pénznemenként (a gyakoriságok havi értékre átszámolva).</summary>
public sealed record MonthlyEstimateDto(string Currency, decimal Amount);

/// <summary>
/// A mentett felismerési eredmény. Az <see cref="AnalyzedAt"/> a legutóbbi elemzés ideje (null, ha még nem volt eredmény);
/// az <see cref="IsStale"/> igaz, ha azóta tranzakciót rögzítettek vagy módosítottak (a törlést nem érzékeli).
/// </summary>
public sealed record RecurringTransactionsDto(
    IReadOnlyList<RecurringTransactionDto> Items,
    IReadOnlyList<MonthlyEstimateDto> MonthlyEstimates,
    DateTime? AnalyzedAt,
    bool IsStale);
