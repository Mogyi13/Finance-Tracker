using System.Globalization;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>Tranzakciós értékek egységes magyar megjelenítése a felületen.</summary>
public static class TransactionFormat
{
    public static readonly CultureInfo Hu = new("hu-HU");

    public static string Date(DateOnly date) => date.ToString("yyyy. MM. dd.", Hu);

    public static string Amount(TransactionDto transaction)
        => $"{(transaction.Type == TransactionType.Income ? "+" : "−")}{transaction.Amount.ToString("N2", Hu)} {transaction.Currency}";
}
