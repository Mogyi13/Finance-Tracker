using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;

namespace FinanceTracker.Application.Features.Imports.PreviewImport;

/// <summary>UC-11 3. lépés támogatása: a fájl fejléce, mintasorai és a javasolt oszlop-hozzárendelés. Semmit nem ment.</summary>
public sealed class PreviewImportHandler(ICurrentUser currentUser)
{
    private const int SampleRowCount = 5;

    public async Task<Result<ImportPreviewDto>> HandleAsync(PreviewImportQuery query)
    {
        if (currentUser.UserId is null)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var file = await ImportFileReader.ReadAsync(query.FileName, query.Content, query.Delimiter);
        if (file.RequestError is not null)
        {
            return file.RequestError;
        }

        if (file.StructureError is not null)
        {
            return Error.Validation(new Dictionary<string, string[]> { ["file"] = new[] { file.StructureError } });
        }

        var table = file.Table!;
        var samples = table.Rows
            .Take(SampleRowCount)
            .Select(r => (IReadOnlyList<string>)r.Fields.Select(f => f.Trim()).ToList())
            .ToList();

        return Result<ImportPreviewDto>.Ok(new ImportPreviewDto(
            query.FileName.Trim(),
            ImportDelimiterExtensions.FromChar(table.Delimiter),
            table.Header,
            samples,
            table.Rows.Count,
            ColumnMappingSuggester.Suggest(table.Header)));
    }
}
