using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Imports.ImportCsv;

/// <summary>
/// UC-11 és UC-12: CSV-import. A sorok ellenőrzése után az érvényes sorok tranzakcióként, a hibás sorok
/// importhibaként, az összesítő pedig importfolyamatként (ImportBatch) kerül mentésre; a részleges siker megengedett.
/// A duplikált sorokat az ujjlenyomatuk alapján kihagyja (lásd <see cref="TransactionFingerprint"/>).
/// </summary>
public sealed class ImportCsvHandler(
    ICurrentUser currentUser,
    ICategoryRepository categories,
    IImportRepository imports,
    ITransactionCategorizer categorizer,
    IDerivedDataInvalidator derivedData,
    TimeProvider timeProvider)
{
    public async Task<Result<ImportResultDto>> HandleAsync(ImportCsvCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var errors = new FieldErrors();
        var mapping = command.Mapping;
        if (string.IsNullOrWhiteSpace(mapping?.DateColumn))
        {
            errors.Add("dateColumn", "A dátum oszlopának kiválasztása kötelező.");
        }

        if (string.IsNullOrWhiteSpace(mapping?.AmountColumn))
        {
            errors.Add("amountColumn", "Az összeg oszlopának kiválasztása kötelező.");
        }

        if (string.IsNullOrWhiteSpace(mapping?.DescriptionColumn))
        {
            errors.Add("descriptionColumn", "A leírás oszlopának kiválasztása kötelező.");
        }

        var defaultCurrency = TransactionFieldRules.NormalizeCurrency(command.DefaultCurrency);
        if (!SupportedCurrencies.IsSupported(defaultCurrency))
        {
            errors.Add("defaultCurrency", $"A pénznem nem támogatott. Támogatott pénznemek: {string.Join(", ", SupportedCurrencies.All)}.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var file = await ImportFileReader.ReadAsync(command.FileName, command.Content, command.Delimiter);
        if (file.RequestError is not null)
        {
            return file.RequestError;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var batch = ImportBatch.Start(Guid.NewGuid(), userId, command.FileName, now);
        var importErrors = new List<ImportError>();

        // --- A fájl szerkezetének ellenőrzése: hiba esetén az import sikertelenként rögzül.
        string? structureError = file.StructureError;
        ColumnIndexes? columns = null;
        if (structureError is null)
        {
            columns = ResolveColumns(file.Table!.Header, mapping!, out structureError);
        }

        if (structureError is not null)
        {
            batch.Fail(timeProvider.GetUtcNow().UtcDateTime);
            importErrors.Add(ImportError.Create(
                Guid.NewGuid(), batch.Id, ImportError.FileLevelRow, ImportErrorCodes.InvalidFile, structureError));

            await imports.SaveAsync(batch, importErrors, Array.Empty<Transaction>());
            return Result<ImportResultDto>.Ok(ToDto(batch, importErrors, new List<int>()));
        }

        var table = file.Table!;
        var available = await categories.GetAvailableAsync(userId);
        var validator = new ImportRowValidator(columns!, defaultCurrency, available);

        // --- UC-12: sorok ellenőrzése
        var validRows = new List<ValidRow>();
        var failedRows = 0;
        foreach (var record in table.Rows)
        {
            if (record.Fields.Length != table.Header.Length)
            {
                failedRows++;
                importErrors.Add(ImportError.Create(
                    Guid.NewGuid(),
                    batch.Id,
                    record.Number,
                    ImportErrorCodes.InvalidRow,
                    $"A sor {record.Fields.Length} oszlopot tartalmaz, a fejléc {table.Header.Length} oszlopot."));
                continue;
            }

            var validation = validator.Validate(record.Number, record.Fields);
            if (validation.IsValid)
            {
                validRows.Add(validation.Row!);
                continue;
            }

            failedRows++;
            importErrors.AddRange(validation.Issues.Select(issue =>
                ImportError.Create(Guid.NewGuid(), batch.Id, record.Number, issue.Code, issue.Message)));
        }

        // --- Duplikációk kihagyása: a már meglévő azonos ujjlenyomatú tranzakciók darabszáma erejéig.
        var duplicateRowNumbers = new List<int>();
        var rowsToImport = new List<ValidRow>(validRows.Count);

        if (command.SkipDuplicates && validRows.Count > 0)
        {
            var from = validRows.Min(r => r.Details.Date);
            var to = validRows.Max(r => r.Details.Date);
            var existing = new Dictionary<string, int>(await imports.GetExistingFingerprintCountsAsync(userId, from, to));

            foreach (var row in validRows)
            {
                var fingerprint = TransactionFingerprint.Compute(
                    row.Details.Date, row.Details.Type, row.Details.Amount, row.Details.Currency, row.Details.Description);

                if (existing.TryGetValue(fingerprint, out var remaining) && remaining > 0)
                {
                    existing[fingerprint] = remaining - 1;
                    duplicateRowNumbers.Add(row.RowNumber);
                }
                else
                {
                    rowsToImport.Add(row);
                }
            }
        }
        else
        {
            rowsToImport.AddRange(validRows);
        }

        // --- Tranzakciók létrehozása
        var transactions = new List<Transaction>(rowsToImport.Count);
        foreach (var row in rowsToImport)
        {
            var transaction = Transaction.Create(Guid.NewGuid(), userId, row.Details, TransactionSource.CsvImport, now);
            await AssignCategoryAsync(transaction, row, available, userId);
            transactions.Add(transaction);
        }

        batch.Complete(
            totalRows: table.Rows.Count,
            successfulRows: transactions.Count,
            failedRows: failedRows,
            duplicateRows: duplicateRowNumbers.Count,
            completedAtUtc: timeProvider.GetUtcNow().UtcDateTime);

        await imports.SaveAsync(batch, importErrors, transactions);

        if (transactions.Count > 0)
        {
            await derivedData.InvalidateAsync(userId, transactions.Select(t => t.TransactionDate).Distinct().ToList());
        }

        return Result<ImportResultDto>.Ok(ToDto(batch, importErrors, duplicateRowNumbers));
    }

    /// <summary>A hozzárendelt oszlopnevek fejléc szerinti indexe; hiányzó oszlop esetén szerkezeti hibaüzenet.</summary>
    private static ColumnIndexes? ResolveColumns(string[] header, CsvColumnMapping mapping, out string? error)
    {
        error = null;
        var missing = new List<string>();

        int Find(string? name, bool required)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return -1;
            }

            var index = Array.FindIndex(header, h => string.Equals(h, name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                missing.Add(name.Trim());
            }

            return index;
        }

        var date = Find(mapping.DateColumn, true);
        var amount = Find(mapping.AmountColumn, true);
        var description = Find(mapping.DescriptionColumn, true);
        var type = Find(mapping.TypeColumn, false);
        var currency = Find(mapping.CurrencyColumn, false);
        var category = Find(mapping.CategoryColumn, false);

        if (missing.Count > 0)
        {
            error = $"A hozzárendelt oszlop(ok) nem találhatók a fájl fejlécében: {string.Join(", ", missing.Select(m => $"„{m}”"))}.";
            return null;
        }

        var used = new[] { date, amount, description, type, currency, category }.Where(i => i >= 0).ToList();
        if (used.Count != used.Distinct().Count())
        {
            error = "Ugyanazt az oszlopot nem lehet több mezőhöz is hozzárendelni.";
            return null;
        }

        return new ColumnIndexes(date, amount, description, type, currency, category);
    }

    /// <summary>
    /// A fájlban megadott kategória kerül hozzárendelésre; ennek hiányában az automatikus kategorizáló javaslata,
    /// ha van. Egyéb esetben a tranzakció kategorizálatlan marad.
    /// </summary>
    private async Task AssignCategoryAsync(Transaction transaction, ValidRow row, IReadOnlyCollection<Category> available, Guid userId)
    {
        if (row.Category is not null)
        {
            transaction.AssignCategory(row.Category, CategorizationStatus.ManuallyAssigned);
            return;
        }

        var suggestion = await categorizer.SuggestAsync(userId, transaction.Type, transaction.Description);
        if (suggestion is null || suggestion.Status is CategorizationStatus.Uncategorized)
        {
            return;
        }

        var category = available.FirstOrDefault(c => c.Id == suggestion.CategoryId);
        if (category is not null && category.IsCompatibleWith(transaction.Type))
        {
            transaction.AssignCategory(category, suggestion.Status);
        }
    }

    private static ImportResultDto ToDto(ImportBatch batch, IReadOnlyList<ImportError> errors, IReadOnlyList<int> duplicateRows)
        => new(
            batch.Id,
            batch.FileName,
            batch.Status,
            batch.TotalRows,
            batch.SuccessfulRows,
            batch.FailedRows,
            batch.DuplicateRows,
            batch.CreatedAt,
            batch.CompletedAt,
            errors.Take(ImportLimits.MaxReportedItems)
                .Select(e => new ImportErrorDto(e.RowNumber, e.ErrorCode, e.ErrorMessage))
                .ToList(),
            errors.Count,
            duplicateRows.Take(ImportLimits.MaxReportedItems).ToList());
}
