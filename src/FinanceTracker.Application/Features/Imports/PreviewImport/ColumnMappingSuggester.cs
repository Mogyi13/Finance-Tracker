namespace FinanceTracker.Application.Features.Imports.PreviewImport;

/// <summary>Oszlop-hozzárendelés javaslata a fejlécnevek alapján (magyar és angol elnevezésekkel).</summary>
internal static class ColumnMappingSuggester
{
    private static readonly string[] DateNames = { "date", "datum", "tranzakciodatuma", "konyvelesdatuma", "bookingdate", "transactiondate", "erteknap" };
    private static readonly string[] AmountNames = { "amount", "osszeg", "ertek", "tranzakciosszeg", "tranzakcioosszege", "sum", "value" };
    private static readonly string[] DescriptionNames = { "description", "leiras", "kozlemeny", "megjegyzes", "partner", "tetel", "name", "megnevezes", "details", "memo" };
    private static readonly string[] TypeNames = { "type", "tipus", "irany", "transactiontype", "tranzakciotipusa" };
    private static readonly string[] CurrencyNames = { "currency", "penznem", "deviza", "valuta" };
    private static readonly string[] CategoryNames = { "category", "kategoria" };

    public static CsvColumnMapping Suggest(IReadOnlyList<string> headers)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? Pick(string[] names)
        {
            foreach (var header in headers)
            {
                if (used.Contains(header))
                {
                    continue;
                }

                if (names.Contains(Normalize(header)))
                {
                    used.Add(header);
                    return header;
                }
            }

            return null;
        }

        return new CsvColumnMapping(
            Pick(DateNames),
            Pick(AmountNames),
            Pick(DescriptionNames),
            Pick(TypeNames),
            Pick(CurrencyNames),
            Pick(CategoryNames));
    }

    /// <summary>Kisbetűs, ékezet- és elválasztójel-mentes alak az összehasonlításhoz.</summary>
    internal static string Normalize(string header)
    {
        var chars = new List<char>(header.Length);
        foreach (var c in header.Trim().ToLowerInvariant())
        {
            var mapped = c switch
            {
                'á' => 'a',
                'é' => 'e',
                'í' => 'i',
                'ó' or 'ö' or 'ő' => 'o',
                'ú' or 'ü' or 'ű' => 'u',
                _ => c
            };

            if (char.IsLetterOrDigit(mapped))
            {
                chars.Add(mapped);
            }
        }

        return new string(chars.ToArray());
    }
}
