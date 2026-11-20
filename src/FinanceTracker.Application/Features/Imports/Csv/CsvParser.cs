using System.Text;

namespace FinanceTracker.Application.Features.Imports.Csv;

/// <summary>A CSV egy rekordja. A sorszám a fájlbeli rekordsorszám (a fejléc az 1.).</summary>
public sealed record CsvRecord(int Number, string[] Fields)
{
    /// <summary>Üres sor: egyetlen mezője sincs, vagy minden mezője üres/szóköz.</summary>
    public bool IsBlank => Fields.All(string.IsNullOrWhiteSpace);
}

public sealed record CsvParseResult(IReadOnlyList<CsvRecord> Records, string? Error)
{
    public bool IsSuccess => Error is null;
}

/// <summary>
/// RFC 4180 szerinti CSV-olvasó: idézőjeles mezők, duplázott idézőjel, sortörés idézőjelen belül.
/// Az elválasztó tetszőleges egykarakteres lehet.
/// </summary>
public static class CsvParser
{
    private static readonly char[] CandidateDelimiters = { ';', ',', '\t', '|' };

    public static CsvParseResult Parse(string text, char delimiter)
    {
        ArgumentNullException.ThrowIfNull(text);

        var records = new List<CsvRecord>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var fieldWasQuoted = false;
        var recordNumber = 1;
        var recordStarted = false;

        var i = text.Length > 0 && text[0] == '﻿' ? 1 : 0;

        void EndField()
        {
            fields.Add(field.ToString());
            field.Clear();
            fieldWasQuoted = false;
        }

        void EndRecord()
        {
            EndField();
            records.Add(new CsvRecord(recordNumber++, fields.ToArray()));
            fields.Clear();
            recordStarted = false;
        }

        while (i < text.Length)
        {
            var c = text[i];
            recordStarted = true;

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                        continue;
                    }

                    inQuotes = false;
                    i++;
                    continue;
                }

                field.Append(c);
                i++;
                continue;
            }

            if (c == '"' && field.Length == 0 && !fieldWasQuoted)
            {
                inQuotes = true;
                fieldWasQuoted = true;
                i++;
            }
            else if (c == delimiter)
            {
                EndField();
                i++;
            }
            else if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                EndRecord();
                i++;
            }
            else
            {
                field.Append(c);
                i++;
            }
        }

        if (inQuotes)
        {
            return new CsvParseResult(records, $"A fájl lezáratlan idézőjelet tartalmaz (a(z) {recordNumber}. rekordnál).");
        }

        if (recordStarted)
        {
            EndRecord();
        }

        return new CsvParseResult(records, null);
    }

    /// <summary>
    /// Az elválasztó kitalálása az első nem üres sorból (az idézőjelen kívüli előfordulások alapján).
    /// Döntetlennél a pontosvessző a magyar Excel-exportok miatt előnyben van; alapértelmezés a vessző.
    /// </summary>
    public static char DetectDelimiter(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var counts = CandidateDelimiters.ToDictionary(d => d, _ => 0);
        var inQuotes = false;
        var sawContent = false;

        foreach (var c in text)
        {
            if (c == '﻿')
            {
                continue;
            }

            if (c == '"')
            {
                inQuotes = !inQuotes;
                sawContent = true;
            }
            else if (!inQuotes && (c == '\r' || c == '\n'))
            {
                if (sawContent)
                {
                    break;
                }
            }
            else if (!inQuotes && counts.ContainsKey(c))
            {
                counts[c]++;
                sawContent = true;
            }
            else if (!char.IsWhiteSpace(c))
            {
                sawContent = true;
            }
        }

        var best = CandidateDelimiters.OrderByDescending(d => counts[d]).First();
        return counts[best] == 0 ? ',' : best;
    }
}
