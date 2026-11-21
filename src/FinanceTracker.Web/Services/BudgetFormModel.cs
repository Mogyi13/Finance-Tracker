using System.ComponentModel.DataAnnotations;
using FinanceTracker.Application.Features.Budgets;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Web.Services;

/// <summary>A költségkeret-űrlap modellje, kliensoldali validációval. A végső ellenőrzést a backend végzi.</summary>
public sealed class BudgetFormModel : IValidatableObject
{
    /// <summary>A kiválasztott kategória azonosítója szövegként; üres, ha nincs kiválasztva.</summary>
    [Required(ErrorMessage = "A kategória kiválasztása kötelező.")]
    public string CategoryKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Az összeg megadása kötelező.")]
    public decimal? Amount { get; set; }

    [Required(ErrorMessage = "A pénznem kiválasztása kötelező.")]
    public string Currency { get; set; } = "HUF";

    public BudgetPeriodType PeriodType { get; set; } = BudgetPeriodType.Monthly;

    [Required(ErrorMessage = "A kezdő dátum megadása kötelező.")]
    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

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

        if (PeriodType == BudgetPeriodType.Custom && EndDate is null)
        {
            yield return new ValidationResult("Az egyedi időszakú költségkeretnek záró dátum kell.", new[] { nameof(EndDate) });
        }

        if (StartDate is { } start && EndDate is { } end && end < start)
        {
            yield return new ValidationResult("A záró dátum nem lehet korábbi a kezdő dátumnál.", new[] { nameof(EndDate) });
        }
    }

    public static BudgetFormModel From(BudgetDto dto) => new()
    {
        CategoryKey = dto.CategoryId.ToString(),
        Amount = dto.Amount,
        Currency = dto.Currency,
        PeriodType = dto.PeriodType,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate
    };

    public SaveBudgetCommand ToCommand()
        => new(Guid.TryParse(CategoryKey, out var id) ? id : Guid.Empty, Amount ?? 0, Currency, PeriodType, StartDate ?? default, EndDate);
}
