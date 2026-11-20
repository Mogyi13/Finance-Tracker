namespace FinanceTracker.Application.Features.Imports;

/// <summary>
/// Az import oszlop-hozzárendelése: a CSV fejlécsorának oszlopnevei. A dátum, az összeg és a leírás
/// kötelező; a típus, a pénznem és a kategória opcionális.
/// </summary>
public sealed record CsvColumnMapping(
    string? DateColumn,
    string? AmountColumn,
    string? DescriptionColumn,
    string? TypeColumn = null,
    string? CurrencyColumn = null,
    string? CategoryColumn = null);
