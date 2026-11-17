using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Transactions.CreateTransaction;

/// <summary>UC-05 kérés: új bevétel vagy kiadás rögzítése.</summary>
public sealed record CreateTransactionCommand(
    TransactionType Type,
    decimal Amount,
    string Currency,
    string? Description,
    DateOnly TransactionDate,
    Guid? CategoryId);
