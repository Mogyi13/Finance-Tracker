using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Imports;

/// <summary>Egy hibás cella (UC-12). Egy sorhoz több is tartozhat, hogy a javítás egy körben elvégezhető legyen.</summary>
internal sealed record RowIssue(string Code, string Message);

/// <summary>Egy érvényes sor: a tranzakció adatai és az opcionálisan megadott kategória.</summary>
internal sealed record ValidRow(int RowNumber, TransactionDetails Details, Category? Category);

internal sealed record RowValidation(ValidRow? Row, IReadOnlyList<RowIssue> Issues)
{
    public bool IsValid => Row is not null;
}

/// <summary>Az oszlopok fejléc szerinti indexei; a nem hozzárendelt opcionális oszlop indexe -1.</summary>
internal sealed record ColumnIndexes(int Date, int Amount, int Description, int Type, int Currency, int Category);

/// <summary>
/// UC-12: egy importált sor ellenőrzése. Kötelező mezők, dátumformátum, pozitív összeg, támogatott pénznem,
/// érvényes típus, szöveghossz és (megadott) kategória. Az érvénytelen sorok nem kerülnek importálásra.
/// </summary>
internal sealed class ImportRowValidator(
    ColumnIndexes columns,
    string defaultCurrency,
    IReadOnlyCollection<Category> availableCategories)
{
    public RowValidation Validate(int rowNumber, string[] fields)
    {
        var issues = new List<RowIssue>();

        // --- Dátum
        DateOnly date = default;
        var dateText = Cell(fields, columns.Date);
        if (string.IsNullOrEmpty(dateText))
        {
            issues.Add(new RowIssue(ImportErrorCodes.InvalidDate, "A dátum megadása kötelező."));
        }
        else if (!ImportValueParser.TryParseDate(dateText, out date))
        {
            issues.Add(new RowIssue(
                ImportErrorCodes.InvalidDate,
                $"A dátum („{Shorten(dateText)}”) nem értelmezhető. Támogatott formátum például: 2026-11-18, 2026. 11. 18., 18.11.2026."));
        }

        // --- Típus (opcionális oszlop; nélküle az összeg előjele dönt)
        TransactionType type = default;
        var typeKnown = false;
        if (columns.Type >= 0)
        {
            var typeText = Cell(fields, columns.Type);
            if (!ImportValueParser.TryParseType(typeText, out type))
            {
                issues.Add(new RowIssue(
                    ImportErrorCodes.InvalidType,
                    string.IsNullOrEmpty(typeText)
                        ? "A típus megadása kötelező (bevétel vagy kiadás)."
                        : $"A típus („{Shorten(typeText)}”) nem értelmezhető. Elfogadott értékek: bevétel / kiadás (income / expense)."));
            }
            else
            {
                typeKnown = true;
            }
        }

        // --- Összeg
        decimal amount = 0;
        var amountKnown = false;
        var amountText = Cell(fields, columns.Amount);
        if (!ImportValueParser.TryParseAmount(amountText, out var parsedAmount, out var amountError))
        {
            issues.Add(new RowIssue(ImportErrorCodes.InvalidAmount, amountError ?? "Az összeg érvénytelen."));
        }
        else if (parsedAmount == 0)
        {
            issues.Add(new RowIssue(ImportErrorCodes.InvalidAmount, "Az összegnek pozitívnak kell lennie."));
        }
        else if (columns.Type >= 0 && parsedAmount < 0)
        {
            // Külön típusoszlop esetén az összeg előjel nélküli; a negatív érték félreértésre utal.
            issues.Add(new RowIssue(
                ImportErrorCodes.InvalidAmount,
                "Az összegnek pozitívnak kell lennie (a típust a típusoszlop adja)."));
        }
        else
        {
            amount = Math.Abs(parsedAmount);
            amountKnown = true;

            if (columns.Type < 0)
            {
                type = parsedAmount < 0 ? TransactionType.Expense : TransactionType.Income;
                typeKnown = true;
            }
        }

        // --- Pénznem
        var currency = defaultCurrency;
        if (columns.Currency >= 0)
        {
            var currencyText = Cell(fields, columns.Currency);
            if (!string.IsNullOrEmpty(currencyText))
            {
                var normalized = ImportValueParser.NormalizeCurrency(currencyText);
                if (normalized is null)
                {
                    issues.Add(new RowIssue(
                        ImportErrorCodes.InvalidCurrency,
                        $"A pénznem („{Shorten(currencyText)}”) nem támogatott. Támogatott pénznemek: {string.Join(", ", Domain.Common.SupportedCurrencies.All)}."));
                }
                else
                {
                    currency = normalized;
                }
            }
        }

        // --- Leírás
        var description = Cell(fields, columns.Description);
        if (string.IsNullOrEmpty(description))
        {
            issues.Add(new RowIssue(ImportErrorCodes.MissingDescription, "A leírás megadása kötelező."));
        }
        else if (description.Length > TransactionDetails.MaxDescriptionLength)
        {
            issues.Add(new RowIssue(
                ImportErrorCodes.InvalidDescription,
                $"A leírás legfeljebb {TransactionDetails.MaxDescriptionLength} karakter lehet (ebben {description.Length} van)."));
        }

        // --- Kategória (opcionális; megadott értéknek létező, elérhető és a típusnak megfelelő kategóriát kell jelölnie)
        Category? category = null;
        if (columns.Category >= 0)
        {
            var categoryText = Cell(fields, columns.Category);
            if (!string.IsNullOrEmpty(categoryText))
            {
                var sameName = availableCategories
                    .Where(c => string.Equals(c.Name, categoryText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (sameName.Count == 0)
                {
                    issues.Add(new RowIssue(
                        ImportErrorCodes.InvalidCategory,
                        $"A(z) „{Shorten(categoryText)}” kategória nem létezik vagy nem érhető el."));
                }
                else if (typeKnown)
                {
                    category = sameName.FirstOrDefault(c => c.IsCompatibleWith(type));
                    if (category is null)
                    {
                        issues.Add(new RowIssue(
                            ImportErrorCodes.InvalidCategory,
                            $"A(z) „{Shorten(categoryText)}” kategória típusa nem egyezik a tranzakció típusával."));
                    }
                }
            }
        }

        if (issues.Count > 0 || !amountKnown || !typeKnown)
        {
            return new RowValidation(null, issues);
        }

        var details = TransactionDetails.Create(type, amount, currency, description, date);
        return new RowValidation(new ValidRow(rowNumber, details, category), issues);
    }

    private static string Cell(string[] fields, int index)
        => index < 0 || index >= fields.Length ? string.Empty : fields[index].Trim();

    private static string Shorten(string value) => value.Length <= 40 ? value : value[..40] + "…";
}
