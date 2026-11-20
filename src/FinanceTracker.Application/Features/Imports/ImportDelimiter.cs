namespace FinanceTracker.Application.Features.Imports;

/// <summary>A CSV-fájl mezőelválasztója. Auto esetén a rendszer a fejlécsorból következteti ki.</summary>
public enum ImportDelimiter
{
    Auto = 0,
    Comma = 1,
    Semicolon = 2,
    Tab = 3,
    Pipe = 4
}

internal static class ImportDelimiterExtensions
{
    public static char ToChar(this ImportDelimiter delimiter) => delimiter switch
    {
        ImportDelimiter.Comma => ',',
        ImportDelimiter.Semicolon => ';',
        ImportDelimiter.Tab => '\t',
        ImportDelimiter.Pipe => '|',
        _ => throw new ArgumentOutOfRangeException(nameof(delimiter), "Az automatikus elválasztónak nincs fix karaktere.")
    };

    public static ImportDelimiter FromChar(char c) => c switch
    {
        ';' => ImportDelimiter.Semicolon,
        '\t' => ImportDelimiter.Tab,
        '|' => ImportDelimiter.Pipe,
        _ => ImportDelimiter.Comma
    };
}
