using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions.GetTransactions;

namespace FinanceTracker.Application.Features.Transactions;

/// <summary>Olvasási oldali lekérdezések, közvetlenül DTO-kat adnak vissza (kategórianévvel együtt).</summary>
public interface ITransactionQueries
{
    Task<TransactionDto?> GetByIdAsync(Guid userId, Guid id);

    Task<PagedResult<TransactionDto>> GetPageAsync(Guid userId, GetTransactionsQuery query);

    /// <summary>A felhasználó kategorizálatlan tranzakciói (legújabb elöl), legfeljebb <paramref name="take"/> darab, az összes darabszámmal.</summary>
    Task<UncategorizedTransactions> GetUncategorizedAsync(Guid userId, int take);
}

public sealed record UncategorizedTransactions(int TotalCount, IReadOnlyList<TransactionDto> Items);
