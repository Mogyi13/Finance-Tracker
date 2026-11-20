namespace FinanceTracker.Application.Features.Imports.GetImportErrors;

/// <summary>UC-14 kérés: egy import soronkénti hibái, lapozva.</summary>
public sealed record GetImportErrorsQuery
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    /// <summary>A lap sorszáma, 1-től.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}
