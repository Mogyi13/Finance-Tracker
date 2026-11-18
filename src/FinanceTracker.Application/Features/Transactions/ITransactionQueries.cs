using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions.GetTransactions;

namespace FinanceTracker.Application.Features.Transactions;

/// <summary>Olvasási oldali lekérdezések, közvetlenül DTO-kat adnak vissza (kategórianévvel együtt).</summary>
public interface ITransactionQueries
{
    Task<TransactionDto?> GetByIdAsync(Guid userId, Guid id);

    Task<PagedResult<TransactionDto>> GetPageAsync(Guid userId, GetTransactionsQuery query);
}
