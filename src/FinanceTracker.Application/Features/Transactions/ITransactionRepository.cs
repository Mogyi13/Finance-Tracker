using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Transactions;

/// <summary>A tranzakció aggregátum írási oldali tárolója. Minden lekérdezés a tulajdonos szerint szűr.</summary>
public interface ITransactionRepository
{
    /// <summary>A saját tranzakció betöltése; más felhasználó tranzakciójára (vagy nem létezőre) null a válasz.</summary>
    Task<Transaction?> GetByIdAsync(Guid userId, Guid id);

    Task AddAsync(Transaction transaction);

    Task UpdateAsync(Transaction transaction);

    Task DeleteAsync(Transaction transaction);
}
