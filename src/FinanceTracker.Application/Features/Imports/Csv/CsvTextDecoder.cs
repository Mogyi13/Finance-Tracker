using System.Text;

namespace FinanceTracker.Application.Features.Imports.Csv;

/// <summary>
/// CSV-bájtok szöveggé alakítása: elsődlegesen UTF-8 (BOM-mal vagy anélkül); ha a tartalom nem érvényes
/// UTF-8, a magyar bankok és az Excel által gyakran használt Windows-1250 kódolással próbálkozik.
/// </summary>
public static class CsvTextDecoder
{
    private const int Windows1250 = 1250;

    public static string Decode(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        try
        {
            return utf8.GetString(bytes).TrimStart('﻿');
        }
        catch (DecoderFallbackException)
        {
            var legacy = CodePagesEncodingProvider.Instance.GetEncoding(Windows1250)
                         ?? Encoding.Latin1;
            return legacy.GetString(bytes);
        }
    }
}
