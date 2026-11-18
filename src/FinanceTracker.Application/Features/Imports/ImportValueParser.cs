using System.Globalization;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Imports;

/// <summary>Az importált cellák értelmezése (dátum, összeg, típus, pénznem). Mind kultúrafüggetlen és determinisztikus.</summary>
internal static class ImportValueParser
{
    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "yyyy.MM.dd.", "yyyy. MM. dd.", "yyyy.MM.dd", "yyyy. MM. dd", "yyyy/MM/dd",
        "dd.MM.yyyy", "dd.MM.yyyy.", "dd/MM/yyyy", "d.M.yyyy", "d/M/yyyy"
    };

    private static readonly HashSet<string> IncomeTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "income", "in", "credit", "bevétel", "bevetel", "jóváírás", "jovairas", "+"
    };

    private static readonly HashSet<string> ExpenseTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "expense", "out", "debit", "kiadás", "kiadas", "terhelés", "terheles", "-", "−"
    };

    private static readonly Dictionary<string, string> CurrencyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FT"] = "HUF",
        ["€"] = "EUR",
        ["$"] = "USD",
        ["£"] = "GBP"
    };

    public static bool TryParseDate(string? value, out DateOnly date)
    {
        date = default;
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (!DateOnly.TryParseExact(trimmed, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return false;
        }

        // Az ésszerűtlen évszámok (például elgépelés) hibásnak számítanak.
        return date.Year is >= 1900 and <= 2100;
    }

    /// <summary>
    /// Az összeg értelmezése. Támogatott: előjel (+, -, −, zárójel), szóköz/NBSP ezres tagolás, pénznemjel vagy -kód,
    /// tizedesvessző vagy -pont, ezres tagolás ponttal/vesszővel. Egyetlen elválasztó tizedesjelnek számít.
    /// </summary>
    public static bool TryParseAmount(string? value, out decimal amount, out string? error)
    {
        amount = 0;
        error = null;

        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            error = "Az összeg megadása kötelező.";
            return false;
        }

        // A pénznem jelölése (Ft, HUF, €) csak az összeg előtt vagy után állhat.
        text = StripCurrencyMarks(text);

        var negative = false;
        if (text.StartsWith('(') && text.EndsWith(')'))
        {
            negative = true;
            text = text[1..^1];
        }

        var cleaned = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c) || c == '\'')
            {
                continue;
            }

            if (c is '-' or '−' or '–')
            {
                if (cleaned.Length > 0)
                {
                    error = "Az összeg formátuma érvénytelen.";
                    return false;
                }

                negative = !negative;
                continue;
            }

            if (c == '+')
            {
                if (cleaned.Length > 0)
                {
                    error = "Az összeg formátuma érvénytelen.";
                    return false;
                }

                continue;
            }

            if (!char.IsAsciiDigit(c) && c != '.' && c != ',')
            {
                error = "Az összeg formátuma érvénytelen.";
                return false;
            }

            cleaned.Append(c);
        }

        var digits = cleaned.ToString();
        if (digits.Length == 0 || !digits.Any(char.IsAsciiDigit))
        {
            error = "Az összeg formátuma érvénytelen.";
            return false;
        }

        var lastDot = digits.LastIndexOf('.');
        var lastComma = digits.LastIndexOf(',');
        char? decimalSeparator = null;
        if (lastDot >= 0 && lastComma >= 0)
        {
            decimalSeparator = lastDot > lastComma ? '.' : ',';
        }
        else if (lastDot >= 0 || lastComma >= 0)
        {
            var separator = lastDot >= 0 ? '.' : ',';
            // Többször előforduló egyetlen elválasztó ezres tagolás; egyszeri előfordulás tizedesjel.
            decimalSeparator = digits.Count(ch => ch == separator) == 1 ? separator : null;
        }

        // Ezres tagolás esetén a csoportok pontosan három számjegyesek (az első 1-3 számjegyű).
        var thousandsSeparator = decimalSeparator switch
        {
            '.' => ',',
            ',' => '.',
            _ => lastDot >= 0 ? '.' : lastComma >= 0 ? ',' : (char?)null
        };

        if (thousandsSeparator is { } ts)
        {
            var integerPart = decimalSeparator is { } ds ? digits[..digits.LastIndexOf(ds)] : digits;
            var groups = integerPart.Split(ts);
            if (groups.Length > 1
                && (groups[0].Length is < 1 or > 3 || groups.Skip(1).Any(g => g.Length != 3)))
            {
                error = "Az összeg formátuma érvénytelen.";
                return false;
            }
        }

        string normalized;
        if (decimalSeparator is { } d)
        {
            var thousands = d == '.' ? ',' : '.';
            normalized = digits.Replace(thousands.ToString(), string.Empty).Replace(d, '.');
        }
        else
        {
            normalized = digits.Replace(".", string.Empty).Replace(",", string.Empty);
        }

        if (normalized.Count(ch => ch == '.') > 1
            || !decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
        {
            error = "Az összeg formátuma érvénytelen.";
            return false;
        }

        if (decimal.Round(parsed, 2) != parsed)
        {
            error = "Az összeg legfeljebb két tizedesjegyet tartalmazhat.";
            return false;
        }

        if (parsed > TransactionDetails.MaxAmount)
        {
            error = "Az összeg túl nagy.";
            return false;
        }

        amount = negative ? -parsed : parsed;
        return true;
    }

    private static string StripCurrencyMarks(string text)
    {
        var start = 0;
        var end = text.Length;
        while (start < end && IsCurrencyMark(text[start]))
        {
            start++;
        }

        while (end > start && IsCurrencyMark(text[end - 1]))
        {
            end--;
        }

        return text[start..end];
    }

    private static bool IsCurrencyMark(char c)
        => char.IsLetter(c) || char.IsWhiteSpace(c) || char.GetUnicodeCategory(c) == UnicodeCategory.CurrencySymbol;

    public static bool TryParseType(string? value, out TransactionType type)
    {
        type = default;
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (IncomeTokens.Contains(trimmed))
        {
            type = TransactionType.Income;
            return true;
        }

        if (ExpenseTokens.Contains(trimmed))
        {
            type = TransactionType.Expense;
            return true;
        }

        return false;
    }

    /// <summary>A pénznem kódja nagybetűsítve (néhány gyakori jelöléssel); null, ha nem támogatott.</summary>
    public static string? NormalizeCurrency(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        var code = CurrencyAliases.TryGetValue(trimmed, out var alias) ? alias : trimmed.ToUpperInvariant();
        return SupportedCurrencies.IsSupported(code) ? code : null;
    }
}
