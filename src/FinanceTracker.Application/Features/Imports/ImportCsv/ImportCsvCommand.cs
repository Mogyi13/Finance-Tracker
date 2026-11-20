namespace FinanceTracker.Application.Features.Imports.ImportCsv;

/// <summary>UC-11 kérés: CSV-fájl importálása a megadott oszlop-hozzárendeléssel.</summary>
public sealed record ImportCsvCommand(
    string FileName,
    Stream Content,
    CsvColumnMapping Mapping,
    ImportDelimiter Delimiter = ImportDelimiter.Auto,
    string DefaultCurrency = "HUF",
    bool SkipDuplicates = true);
