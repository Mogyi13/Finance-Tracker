namespace FinanceTracker.Application.Features.Imports;

/// <summary>Az importálási hibák azonosítói (ImportErrors.ErrorCode).</summary>
public static class ImportErrorCodes
{
    /// <summary>A fájl szerkezete nem feldolgozható (fájl szintű hiba, 0. sor).</summary>
    public const string InvalidFile = "INVALID_FILE";

    /// <summary>A sor oszlopszáma nem egyezik a fejlécével.</summary>
    public const string InvalidRow = "INVALID_ROW";

    public const string InvalidDate = "INVALID_DATE";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string MissingDescription = "MISSING_DESCRIPTION";
    public const string InvalidDescription = "INVALID_DESCRIPTION";
    public const string InvalidCurrency = "INVALID_CURRENCY";
    public const string InvalidType = "INVALID_TYPE";
    public const string InvalidCategory = "INVALID_CATEGORY";
}
