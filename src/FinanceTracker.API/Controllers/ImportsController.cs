using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Imports;
using FinanceTracker.Application.Features.Imports.ImportCsv;
using FinanceTracker.Application.Features.Imports.PreviewImport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>A CSV-fájl feltöltése és a hozzá tartozó oszlop-hozzárendelés (multipart/form-data).</summary>
public sealed class ImportCsvForm
{
    public IFormFile? File { get; set; }

    public string? DateColumn { get; set; }

    public string? AmountColumn { get; set; }

    public string? DescriptionColumn { get; set; }

    public string? TypeColumn { get; set; }

    public string? CurrencyColumn { get; set; }

    public string? CategoryColumn { get; set; }

    public ImportDelimiter Delimiter { get; set; } = ImportDelimiter.Auto;

    public string DefaultCurrency { get; set; } = "HUF";

    public bool SkipDuplicates { get; set; } = true;
}

public sealed class PreviewImportForm
{
    public IFormFile? File { get; set; }

    public ImportDelimiter Delimiter { get; set; } = ImportDelimiter.Auto;
}

[ApiController]
[Authorize]
[Route("api/imports")]
public class ImportsController(PreviewImportHandler preview, ImportCsvHandler import) : ControllerBase
{
    // A fájlméret-korlátot az Application ellenőrzi; a kérés korlátja ennél kicsit nagyobb (űrlapmezők, határolók).
    private const long RequestLimitBytes = ImportLimits.MaxFileSizeBytes + 512 * 1024;

    /// <summary>UC-11 előkészítő lépés: a fájl fejléce, mintasorai és a javasolt oszlop-hozzárendelés. Nem ment semmit.</summary>
    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(RequestLimitBytes)]
    [ProducesResponseType<ImportPreviewDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Preview([FromForm] PreviewImportForm form)
    {
        if (form.File is null)
        {
            return this.ToProblem(MissingFile());
        }

        await using var stream = form.File.OpenReadStream();
        var result = await preview.HandleAsync(new PreviewImportQuery(form.File.FileName, stream, form.Delimiter));
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>
    /// UC-11 és UC-12: CSV-import. Az import minden esetben rögzül (akkor is, ha sikertelen), az eredménye
    /// a válaszban szerepel; a részleges siker megengedett.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(RequestLimitBytes)]
    [ProducesResponseType<ImportResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import([FromForm] ImportCsvForm form)
    {
        if (form.File is null)
        {
            return this.ToProblem(MissingFile());
        }

        var mapping = new CsvColumnMapping(
            form.DateColumn,
            form.AmountColumn,
            form.DescriptionColumn,
            form.TypeColumn,
            form.CurrencyColumn,
            form.CategoryColumn);

        await using var stream = form.File.OpenReadStream();
        var result = await import.HandleAsync(new ImportCsvCommand(
            form.File.FileName, stream, mapping, form.Delimiter, form.DefaultCurrency, form.SkipDuplicates));

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToProblem(result.Error!);
    }

    private static FinanceTracker.Application.Common.Error MissingFile()
        => FinanceTracker.Application.Common.Error.Validation(
            new Dictionary<string, string[]> { ["file"] = new[] { "A CSV-fájl megadása kötelező." } });
}
