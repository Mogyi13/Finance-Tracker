using System.Net.Http.Headers;
using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Imports;
using FinanceTracker.Application.Features.Imports.PreviewImport;

namespace FinanceTracker.Web.Services;

/// <summary>Az API importvégpontjainak kliense (UC-11, UC-12): a fájl multipart/form-data űrlapként utazik.</summary>
public sealed class ImportApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    public Task<ApiResult<ImportPreviewDto>> PreviewAsync(string fileName, byte[] content, ImportDelimiter delimiter)
    {
        var form = new MultipartFormDataContent
        {
            { FileContent(content), "file", fileName },
            { new StringContent(delimiter.ToString()), "delimiter" }
        };

        return SendContentAsync<ImportPreviewDto>(HttpMethod.Post, "api/imports/preview", form);
    }

    public Task<ApiResult<ImportResultDto>> ImportAsync(
        string fileName,
        byte[] content,
        CsvColumnMapping mapping,
        ImportDelimiter delimiter,
        string defaultCurrency,
        bool skipDuplicates)
    {
        var form = new MultipartFormDataContent
        {
            { FileContent(content), "file", fileName },
            { new StringContent(delimiter.ToString()), "delimiter" },
            { new StringContent(defaultCurrency), "defaultCurrency" },
            { new StringContent(skipDuplicates ? "true" : "false"), "skipDuplicates" }
        };

        AddIfPresent(form, "dateColumn", mapping.DateColumn);
        AddIfPresent(form, "amountColumn", mapping.AmountColumn);
        AddIfPresent(form, "descriptionColumn", mapping.DescriptionColumn);
        AddIfPresent(form, "typeColumn", mapping.TypeColumn);
        AddIfPresent(form, "currencyColumn", mapping.CurrencyColumn);
        AddIfPresent(form, "categoryColumn", mapping.CategoryColumn);

        return SendContentAsync<ImportResultDto>(HttpMethod.Post, "api/imports", form);
    }

    /// <summary>UC-14: a korábbi importok lapozott listája.</summary>
    public Task<ApiResult<PagedResult<ImportBatchDto>>> GetPageAsync(int page, int pageSize)
        => SendAsync<PagedResult<ImportBatchDto>>(HttpMethod.Get, $"api/imports?page={page}&pageSize={pageSize}");

    /// <summary>UC-14: egy import összesítője.</summary>
    public Task<ApiResult<ImportBatchDto>> GetAsync(Guid id)
        => SendAsync<ImportBatchDto>(HttpMethod.Get, $"api/imports/{id}");

    /// <summary>UC-14: egy import soronkénti hibái.</summary>
    public Task<ApiResult<PagedResult<ImportErrorDto>>> GetErrorsAsync(Guid id, int page, int pageSize)
        => SendAsync<PagedResult<ImportErrorDto>>(HttpMethod.Get, $"api/imports/{id}/errors?page={page}&pageSize={pageSize}");

    private static ByteArrayContent FileContent(byte[] content)
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        return file;
    }

    private static void AddIfPresent(MultipartFormDataContent form, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            form.Add(new StringContent(value), name);
        }
    }
}
