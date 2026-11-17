using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Transactions.UpdateTransaction;

/// <summary>UC-07 kérés: a tranzakció módosítható adatainak teljes cseréje (PUT).</summary>
public sealed record UpdateTransactionCommand(
    TransactionType Type,
    decimal Amount,
    string Currency,
    string? Description,
    DateOnly TransactionDate,
    Guid? CategoryId);
