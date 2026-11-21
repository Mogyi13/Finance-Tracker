using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// UC-26: kiadási költségkeret egy kategóriára és pénznemre.
/// Havi keretnél a kezdő nap a hónap első napja, heti keretnél a hét hétfője (a kezdő dátum ezekre igazodik);
/// az egyedi keretnek kötelező a záró dátuma. A havi és heti keret a záró dátumig (ha van) minden időszakban érvényes.
/// </summary>
public sealed class Budget
{
    // Az EF Core számára.
    private Budget()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid CategoryId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public BudgetPeriodType PeriodType { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Budget Create(
        Guid id,
        Guid userId,
        Guid categoryId,
        decimal amount,
        string currency,
        BudgetPeriodType periodType,
        DateOnly startDate,
        DateOnly? endDate,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty || categoryId == Guid.Empty)
        {
            throw new DomainException("A költségkeretnek azonosító, tulajdonos és kategória kell.");
        }

        var budget = new Budget
        {
            Id = id,
            UserId = userId,
            CreatedAt = createdAtUtc
        };
        budget.Apply(categoryId, amount, currency, periodType, startDate, endDate);
        return budget;
    }

    public void Update(
        Guid categoryId,
        decimal amount,
        string currency,
        BudgetPeriodType periodType,
        DateOnly startDate,
        DateOnly? endDate,
        DateTime updatedAtUtc)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("A költségkeretnek kategória kell.");
        }

        Apply(categoryId, amount, currency, periodType, startDate, endDate);
        UpdatedAt = updatedAtUtc;
    }

    /// <summary>A kezdő dátum igazítása az időszak típusához (havi: hónap első napja, heti: a hét hétfője).</summary>
    public static DateOnly NormalizeStart(BudgetPeriodType periodType, DateOnly date) => periodType switch
    {
        BudgetPeriodType.Monthly => new DateOnly(date.Year, date.Month, 1),
        BudgetPeriodType.Weekly => date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
        _ => date
    };

    private void Apply(Guid categoryId, decimal amount, string currency, BudgetPeriodType periodType, DateOnly startDate, DateOnly? endDate)
    {
        if (amount <= 0)
        {
            throw new DomainException("A költségkeret összegének pozitívnak kell lennie.");
        }

        if (!TransactionDetails.IsValidAmount(amount))
        {
            throw new DomainException("A költségkeret összege legfeljebb két tizedesjegyet tartalmazhat, és nem lehet túl nagy.");
        }

        if (currency is not { Length: 3 })
        {
            throw new DomainException("A pénznem háromkarakteres kód.");
        }

        if (!Enum.IsDefined(periodType))
        {
            throw new DomainException("Az időszak típusa érvénytelen.");
        }

        if (startDate == default)
        {
            throw new DomainException("A kezdő dátum kötelező.");
        }

        if (periodType == BudgetPeriodType.Custom && endDate is null)
        {
            throw new DomainException("Az egyedi időszakú költségkeretnek záró dátum kell.");
        }

        var start = NormalizeStart(periodType, startDate);
        if (endDate is { } end && start > end)
        {
            throw new DomainException("A kezdő dátum nem lehet későbbi a záró dátumnál.");
        }

        CategoryId = categoryId;
        Amount = amount;
        Currency = currency;
        PeriodType = periodType;
        StartDate = start;
        EndDate = endDate;
    }
}
