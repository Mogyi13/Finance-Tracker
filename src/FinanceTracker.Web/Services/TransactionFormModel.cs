using System.ComponentModel.DataAnnotations;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Application.Features.Transactions.CreateTransaction;
using FinanceTracker.Application.Features.Transactions.UpdateTransaction;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Web.Services;

/// <summary>A tranzakciós űrlap modellje, kliensoldali validációval. A végső ellenőrzést a backend végzi.</summary>
public sealed class TransactionFormModel : IValidatableObject
{
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Az összeg megadása kötelező.")]
    public decimal? Amount { get; set; }

    [Required(ErrorMessage = "A pénznem kiválasztása kötelező.")]
    public string Currency { get; set; } = "HUF";

    [StringLength(TransactionDetails.MaxDescriptionLength, ErrorMessage = "A leírás legfeljebb 500 karakter lehet.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "A dátum megadása kötelező.")]
    public DateOnly? TransactionDate { get; set; }

    /// <summary>A kiválasztott kategória azonosítója szövegként; üres, ha nincs kategória.</summary>
    public string CategoryKey { get; set; } = string.Empty;

    public Guid? CategoryId => Guid.TryParse(CategoryKey, out var id) ? id : null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Amount is { } amount)
        {
            if (amount <= 0)
            {
                yield return new ValidationResult("Az összegnek pozitívnak kell lennie.", new[] { nameof(Amount) });
            }
            else if (!TransactionDetails.IsValidAmount(amount))
            {
                yield return new ValidationResult(
                    "Az összeg legfeljebb két tizedesjegyet tartalmazhat, és nem lehet túl nagy.",
                    new[] { nameof(Amount) });
            }
        }
    }

    public static TransactionFormModel From(TransactionDto dto) => new()
    {
        Type = dto.Type,
        Amount = dto.Amount,
        Currency = dto.Currency,
        Description = dto.Description,
        TransactionDate = dto.TransactionDate,
        CategoryKey = dto.CategoryId?.ToString() ?? string.Empty
    };

    public CreateTransactionCommand ToCreateCommand()
        => new(Type, Amount ?? 0, Currency, Description, TransactionDate ?? default, CategoryId);

    public UpdateTransactionCommand ToUpdateCommand()
        => new(Type, Amount ?? 0, Currency, Description, TransactionDate ?? default, CategoryId);
}
