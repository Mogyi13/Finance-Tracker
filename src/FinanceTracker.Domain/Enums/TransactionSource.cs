namespace FinanceTracker.Domain.Enums;

/// <summary>A tranzakció rögzítésének forrása.</summary>
public enum TransactionSource : byte
{
    Manual = 1,
    CsvImport = 2
}
