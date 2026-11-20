namespace FinanceTracker.Application.Features.Imports.PreviewImport;

/// <summary>A feltöltött fájl előnézete az oszlop-hozzárendelés megerősítéséhez.</summary>
public sealed record ImportPreviewDto(
    string FileName,
    ImportDelimiter Delimiter,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> SampleRows,
    int TotalRows,
    CsvColumnMapping SuggestedMapping);
