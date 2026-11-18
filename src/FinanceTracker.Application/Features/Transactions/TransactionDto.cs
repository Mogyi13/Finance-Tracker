using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Transactions;

public sealed record TransactionDto(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    string Currency,
    string? Description,
    DateOnly TransactionDate,
    Guid? CategoryId,
    string? CategoryName,
    CategorizationStatus CategorizationStatus,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    internal static TransactionDto From(Transaction transaction, string? categoryName)
        => new(
            transaction.Id,
            transaction.Type,
            transaction.Amount,
            transaction.Currency,
            transaction.Description,
            transaction.TransactionDate,
            transaction.CategoryId,
            categoryName,
            transaction.CategorizationStatus,
            transaction.CreatedAt,
            transaction.UpdatedAt);
}
