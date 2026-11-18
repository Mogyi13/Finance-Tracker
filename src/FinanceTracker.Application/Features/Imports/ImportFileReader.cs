using FinanceTracker.Application.Common;
using FinanceTracker.Application.Features.Imports.Csv;
using System.Net.ServerSentEvents;

namespace FinanceTracker.Application.Features.Imports;

/// <summary>A feltöltött CSV beolvasott, fejléc és adatsorok szerint tagolt tartalma.</summary>
internal sealed record CsvTable(char Delimiter, string[] Header, IReadOnlyList<CsvRecord> Rows);

/// <summary>
/// A beolvasás eredménye. A <see cref="RequestError"/> a kérés hibája (nem támogatott formátum, üres vagy túl nagy fájl),
/// ilyenkor nem jön létre import. A <see cref="StructureError"/> a fájl szerkezeti hibája: az import sikertelenként rögzül.
/// </summary>
internal sealed record ImportFileReadResult(CsvTable? Table, Error? RequestError, string? StructureError)
{
    public static ImportFileReadResult Request(string field, string message)
        => new(null, Error.Validation(new Dictionary<string, string[]> { [field] = new[] { message } }), null);

    public static ImportFileReadResult Structure(string message) => new(null, null, message);

    public static ImportFileReadResult Ok(CsvTable table) => new(table, null, null);
}

internal static class ImportFileReader
{
    public static async Task<ImportFileReadResult> ReadAsync(string? fileName, Stream? content, ImportDelimiter delimiterOption)
    {
        if (string.IsNullOrWhiteSpace(fileName) || content is null)
        {
            return ImportFileReadResult.Request("file", "A CSV-fájl megadása kötelező.");
        }

        if (!Enum.IsDefined(delimiterOption))
        {
            return ImportFileReadResult.Request("delimiter", "Az elválasztó karakter érvénytelen.");
        }

        if (!fileName.Trim().EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return ImportFileReadResult.Request("file", "Csak .csv kiterjesztésű fájl importálható.");
        }

        var bytes = await ReadLimitedAsync(content);
        if (bytes is null)
        {
            return ImportFileReadResult.Request(
                "file",
                $"A fájl túl nagy, legfeljebb {ImportLimits.MaxFileSizeBytes / (1024 * 1024)} MB méretű fájl tölthető fel.");
        }

        var text = CsvTextDecoder.Decode(bytes);
        if (string.IsNullOrWhiteSpace(text))
        {
            return ImportFileReadResult.Request("file", "A fájl üres.");
        }

        var delimiter = delimiterOption == ImportDelimiter.Auto
            ? CsvParser.DetectDelimiter(text)
            : delimiterOption.ToChar();

        var parsed = CsvParser.Parse(text, delimiter);
        if (!parsed.IsSuccess)
        {
            return ImportFileReadResult.Structure(parsed.Error!);
        }

        var records = parsed.Records;
        var headerRecord = records.FirstOrDefault(r => !r.IsBlank);
        if (headerRecord is null)
        {
            return ImportFileReadResult.Request("file", "A fájl üres.");
        }

        var header = headerRecord.Fields.Select(h => h.Trim()).ToArray();
        if (header.Any(string.IsNullOrEmpty))
        {
            return ImportFileReadResult.Structure("A fejlécsor üres oszlopnevet tartalmaz.");
        }

        var duplicate = header.GroupBy(h => h, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            return ImportFileReadResult.Structure($"A fejlécsorban ismétlődő oszlopnév van: „{duplicate.Key}”.");
        }

        var rows = records.Where(r => r.Number > headerRecord.Number && !r.IsBlank).ToList();
        if (rows.Count == 0)
        {
            return ImportFileReadResult.Structure("A fájl a fejlécsoron kívül nem tartalmaz adatsort.");
        }

        if (rows.Count > ImportLimits.MaxDataRows)
        {
            return ImportFileReadResult.Structure(
                $"A fájl legfeljebb {ImportLimits.MaxDataRows} adatsort tartalmazhat, ebben {rows.Count} van.");
        }

        return ImportFileReadResult.Ok(new CsvTable(delimiter, header, rows));
    }

    /// <summary>A tartalom beolvasása a méretkorlátig; null, ha a korlátot meghaladja.</summary>
    private static async Task<byte[]?> ReadLimitedAsync(Stream content)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(chunk)) > 0)
        {
            buffer.Write(chunk, 0, read);
            if (buffer.Length > ImportLimits.MaxFileSizeBytes)
            {
                return null;
            }
        }

        return buffer.ToArray();
    }
}
