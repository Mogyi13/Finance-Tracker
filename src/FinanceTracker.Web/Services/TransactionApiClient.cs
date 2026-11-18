using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Application.Features.Transactions.CreateTransaction;
using FinanceTracker.Application.Features.Transactions.GetTransactions;
using FinanceTracker.Application.Features.Transactions.UpdateTransaction;

namespace FinanceTracker.Web.Services;

/// <summary>Az API tranzakciós végpontjainak kliense (UC-05–UC-08).</summary>
public sealed class TransactionApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    public Task<ApiResult<PagedResult<TransactionDto>>> GetPageAsync(GetTransactionsQuery query)
        => SendAsync<PagedResult<TransactionDto>>(
            HttpMethod.Get,
            $"api/transactions?page={query.Page}&pageSize={query.PageSize}&sortBy={query.SortBy}&sortDirection={query.SortDirection}");

    public Task<ApiResult<TransactionDto>> GetAsync(Guid id)
        => SendAsync<TransactionDto>(HttpMethod.Get, $"api/transactions/{id}");

    public Task<ApiResult<TransactionDto>> CreateAsync(CreateTransactionCommand command)
        => SendAsync<TransactionDto>(HttpMethod.Post, "api/transactions", command);

    public Task<ApiResult<TransactionDto>> UpdateAsync(Guid id, UpdateTransactionCommand command)
        => SendAsync<TransactionDto>(HttpMethod.Put, $"api/transactions/{id}", command);

    public Task<ApiResult<bool>> DeleteAsync(Guid id)
        => SendNoContentAsync(HttpMethod.Delete, $"api/transactions/{id}");
}
