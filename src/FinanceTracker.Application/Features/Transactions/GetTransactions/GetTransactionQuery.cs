using FinanceTracker.Application.Common.Pagination;

namespace FinanceTracker.Application.Features.Transactions.GetTransactions;

/// <summary>UC-06 kérés: a saját tranzakciók lapozott, rendezett listája.</summary>
public sealed record GetTransactionsQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>A lap sorszáma, 1-től.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;

    public TransactionSortField SortBy { get; init; } = TransactionSortField.Date;

    public SortDirection SortDirection { get; init; } = SortDirection.Descending;
}
