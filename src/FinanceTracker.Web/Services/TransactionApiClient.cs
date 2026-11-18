using System.Globalization;
using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Application.Features.Transactions.CreateTransaction;
using FinanceTracker.Application.Features.Transactions.GetTransactions;
using FinanceTracker.Application.Features.Transactions.UpdateTransaction;

namespace FinanceTracker.Web.Services;

/// <summary>Az API tranzakciós végpontjainak kliense (UC-05–UC-09).</summary>
public sealed class TransactionApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    public Task<ApiResult<PagedResult<TransactionDto>>> GetPageAsync(GetTransactionsQuery query)
        => SendAsync<PagedResult<TransactionDto>>(HttpMethod.Get, $"api/transactions?{BuildQueryString(query)}");

    /// <summary>A lista lekérdezési paraméterei (kultúrafüggetlen formátumban, URL-kódolva). Csak a megadott szűrők kerülnek bele.</summary>
    internal static string BuildQueryString(GetTransactionsQuery query)
    {
        var parts = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}",
            $"sortBy={query.SortBy}",
            $"sortDirection={query.SortDirection}"
        };

        void Add(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }

        Add("dateFrom", query.DateFrom?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add("dateTo", query.DateTo?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Add("type", query.Type?.ToString());
        Add("categoryId", query.CategoryId?.ToString());
        Add("minAmount", query.MinAmount?.ToString(CultureInfo.InvariantCulture));
        Add("maxAmount", query.MaxAmount?.ToString(CultureInfo.InvariantCulture));
        Add("search", query.NormalizedSearch);

        return string.Join('&', parts);
    }

    public Task<ApiResult<TransactionDto>> GetAsync(Guid id)
        => SendAsync<TransactionDto>(HttpMethod.Get, $"api/transactions/{id}");

    public Task<ApiResult<TransactionDto>> CreateAsync(CreateTransactionCommand command)
        => SendAsync<TransactionDto>(HttpMethod.Post, "api/transactions", command);

    public Task<ApiResult<TransactionDto>> UpdateAsync(Guid id, UpdateTransactionCommand command)
        => SendAsync<TransactionDto>(HttpMethod.Put, $"api/transactions/{id}", command);

    public Task<ApiResult<bool>> DeleteAsync(Guid id)
        => SendNoContentAsync(HttpMethod.Delete, $"api/transactions/{id}");
}
