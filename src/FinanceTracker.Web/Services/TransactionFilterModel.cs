using System.ComponentModel.DataAnnotations;
using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions.GetTransactions;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>A tranzakciólista szűrőűrlapjának modellje (UC-09), kliensoldali ellenőrzéssel.</summary>
public sealed class TransactionFilterModel : IValidatableObject
{
    public DateOnly? DateFrom { get; set; }

    public DateOnly? DateTo { get; set; }

    /// <summary>"" (mind), "Income" vagy "Expense".</summary>
    public string TypeKey { get; set; } = string.Empty;

    /// <summary>A kategória azonosítója szövegként; üres, ha nincs kategóriaszűrő.</summary>
    public string CategoryKey { get; set; } = string.Empty;

    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }

    [StringLength(GetTransactionsQuery.MaxSearchLength, ErrorMessage = "A keresőszöveg legfeljebb 100 karakter lehet.")]
    public string? Search { get; set; }

    public TransactionType? Type => Enum.TryParse<TransactionType>(TypeKey, out var type) ? type : null;

    public Guid? CategoryId => Guid.TryParse(CategoryKey, out var id) ? id : null;

    /// <summary>A megadott szűrők száma (a jelvényhez).</summary>
    public int ActiveCount
        => (DateFrom is null ? 0 : 1)
           + (DateTo is null ? 0 : 1)
           + (Type is null ? 0 : 1)
           + (CategoryId is null ? 0 : 1)
           + (MinAmount is null ? 0 : 1)
           + (MaxAmount is null ? 0 : 1)
           + (string.IsNullOrWhiteSpace(Search) ? 0 : 1);

    public bool IsActive => ActiveCount > 0;

    public TransactionFilterModel Clone() => (TransactionFilterModel)MemberwiseClone();

    public GetTransactionsQuery ToQuery(int page, int pageSize, TransactionSortField sortBy, SortDirection sortDirection)
        => new()
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection,
            DateFrom = DateFrom,
            DateTo = DateTo,
            Type = Type,
            CategoryId = CategoryId,
            MinAmount = MinAmount,
            MaxAmount = MaxAmount,
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim()
        };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateFrom is { } from && DateTo is { } to && from > to)
        {
            yield return new ValidationResult("A záró dátum nem lehet korábbi a kezdő dátumnál.", new[] { nameof(DateTo) });
        }

        if (MinAmount is < 0)
        {
            yield return new ValidationResult("Az összeg nem lehet negatív.", new[] { nameof(MinAmount) });
        }

        if (MaxAmount is < 0)
        {
            yield return new ValidationResult("Az összeg nem lehet negatív.", new[] { nameof(MaxAmount) });
        }

        if (MinAmount is { } min && MaxAmount is { } max && min > max)
        {
            yield return new ValidationResult("A legnagyobb összeg nem lehet kisebb a legkisebbnél.", new[] { nameof(MaxAmount) });
        }
    }
}
