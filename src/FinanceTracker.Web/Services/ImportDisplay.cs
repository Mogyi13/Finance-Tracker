using FinanceTracker.Application.Features.Imports;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>Az importok állapotának és hibakódjainak magyar megjelenítése (importvarázsló, előzmények, részletek).</summary>
public static class ImportDisplay
{
    public static string DelimiterName(ImportDelimiter delimiter) => delimiter switch
    {
        ImportDelimiter.Comma => "vessző (,)",
        ImportDelimiter.Semicolon => "pontosvessző (;)",
        ImportDelimiter.Tab => "tabulátor",
        ImportDelimiter.Pipe => "függőleges vonal (|)",
        _ => "automatikus"
    };

    /// <summary>success | warning | danger | info – a StatusMessage és a státuszcímke színéhez.</summary>
    public static string StatusKind(ImportBatchStatus status) => status switch
    {
        ImportBatchStatus.Completed => "success",
        ImportBatchStatus.PartiallyCompleted => "warning",
        ImportBatchStatus.Failed => "danger",
        _ => "info"
    };

    public static string StatusText(ImportBatchStatus status) => status switch
    {
        ImportBatchStatus.Pending => "Várakozik",
        ImportBatchStatus.Processing => "Feldolgozás alatt",
        ImportBatchStatus.Completed => "Sikeres",
        ImportBatchStatus.PartiallyCompleted => "Részben sikeres",
        ImportBatchStatus.Failed => "Sikertelen",
        _ => status.ToString()
    };

    public static string ResultTitle(ImportBatchStatus status) => status switch
    {
        ImportBatchStatus.Completed => "Az import sikeresen lefutott.",
        ImportBatchStatus.PartiallyCompleted => "Az import részben sikerült: a hibás sorok kimaradtak.",
        ImportBatchStatus.Failed => "Az import sikertelen.",
        _ => "Az import folyamatban van."
    };

    public static string ErrorLabel(string code) => code switch
    {
        ImportErrorCodes.InvalidFile => "Fájl szerkezete",
        ImportErrorCodes.InvalidRow => "Oszlopszám",
        ImportErrorCodes.InvalidDate => "Dátum",
        ImportErrorCodes.InvalidAmount => "Összeg",
        ImportErrorCodes.MissingDescription => "Leírás hiányzik",
        ImportErrorCodes.InvalidDescription => "Leírás",
        ImportErrorCodes.InvalidCurrency => "Pénznem",
        ImportErrorCodes.InvalidType => "Típus",
        ImportErrorCodes.InvalidCategory => "Kategória",
        _ => code
    };

    public static string RowLabel(int rowNumber) => rowNumber == 0 ? "Fájl" : rowNumber.ToString();
}
