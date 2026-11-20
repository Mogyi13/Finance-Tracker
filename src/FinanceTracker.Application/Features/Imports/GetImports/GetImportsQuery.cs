namespace FinanceTracker.Application.Features.Imports.GetImports;

/// <summary>UC-14 kérés: a saját importok lapozott listája, a legutóbbi elöl.</summary>
public sealed record GetImportsQuery
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    /// <summary>A lap sorszáma, 1-től.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}
