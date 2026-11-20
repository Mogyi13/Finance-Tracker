using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Transactions.GetTransactions;

/// <summary>UC-06 és UC-09 kérés: a saját tranzakciók lapozott, rendezett és szűrhető listája.</summary>
public sealed record GetTransactionsQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 100;

    /// <summary>A lap sorszáma, 1-től.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;

    public TransactionSortField SortBy { get; init; } = TransactionSortField.Date;

    public SortDirection SortDirection { get; init; } = SortDirection.Descending;

    // --- UC-09 szűrők (mind opcionális, együtt ÉS kapcsolatban érvényesülnek)

    /// <summary>A tranzakció dátuma legalább ez (a határ is beleértendő).</summary>
    public DateOnly? DateFrom { get; init; }

    /// <summary>A tranzakció dátuma legfeljebb ez (a határ is beleértendő).</summary>
    public DateOnly? DateTo { get; init; }

    public TransactionType? Type { get; init; }

    public Guid? CategoryId { get; init; }

    /// <summary>Legkisebb összeg (a határ is beleértendő).</summary>
    public decimal? MinAmount { get; init; }

    /// <summary>Legnagyobb összeg (a határ is beleértendő).</summary>
    public decimal? MaxAmount { get; init; }

    /// <summary>Kulcsszó a leírásban (kis- és nagybetűre érzéketlen, részszóra is illeszkedik).</summary>
    public string? Search { get; init; }

    /// <summary>A keresőszöveg levágva; üres esetén null.</summary>
    public string? NormalizedSearch => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

    /// <summary>Van-e bármilyen szűrő megadva.</summary>
    public bool HasFilters
        => DateFrom is not null || DateTo is not null || Type is not null || CategoryId is not null
           || MinAmount is not null || MaxAmount is not null || NormalizedSearch is not null;
}
