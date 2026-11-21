using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Budgets;

/// <summary>A költségkeret létrehozásának és módosításának közös ellenőrzései.</summary>
internal static class BudgetRules
{
    /// <summary>Mezőszintű ellenőrzés; hibamentes esetén a normalizált adatokat adja vissza.</summary>
    public static async Task<(SaveBudgetCommand? Command, Error? Error)> ValidateAsync(
        SaveBudgetCommand command,
        Guid userId,
        ICategoryRepository categories,
        IBudgetRepository budgets,
        Guid? excludeBudgetId)
    {
        var errors = new FieldErrors();
        var currency = TransactionFieldRules.NormalizeCurrency(command.Currency);

        if (command.Amount <= 0)
        {
            errors.Add("amount", "Az összegnek pozitívnak kell lennie.");
        }
        else if (command.Amount > TransactionDetails.MaxAmount)
        {
            errors.Add("amount", "Az összeg túl nagy.");
        }
        else if (decimal.Round(command.Amount, 2) != command.Amount)
        {
            errors.Add("amount", "Az összeg legfeljebb két tizedesjegyet tartalmazhat.");
        }

        if (!SupportedCurrencies.IsSupported(currency))
        {
            errors.Add("currency", $"A pénznem nem támogatott. Támogatott pénznemek: {string.Join(", ", SupportedCurrencies.All)}.");
        }

        if (!Enum.IsDefined(command.PeriodType))
        {
            errors.Add("periodType", "Az időszak típusa érvénytelen.");
        }

        if (command.StartDate == default)
        {
            errors.Add("startDate", "A kezdő dátum kötelező.");
        }
        else if (Enum.IsDefined(command.PeriodType))
        {
            var start = Budget.NormalizeStart(command.PeriodType, command.StartDate);
            if (command.PeriodType == BudgetPeriodType.Custom && command.EndDate is null)
            {
                errors.Add("endDate", "Az egyedi időszakú költségkeretnek záró dátum kell.");
            }
            else if (command.EndDate is { } end && start > end)
            {
                errors.Add("endDate", "A záró dátum nem lehet korábbi a kezdő dátumnál.");
            }
            else if (command.EndDate is { } e && e.DayNumber - start.DayNumber + 1 > 3660)
            {
                errors.Add("endDate", "Az időszak legfeljebb 3660 napos lehet.");
            }
        }

        // A más felhasználó kategóriája ugyanazt az üzenetet kapja, mint a nem létező (nem derül ki a létezése).
        var category = await categories.GetByIdAsync(command.CategoryId);
        if (category is null || !category.IsAvailableTo(userId))
        {
            errors.Add("categoryId", "A kategória nem létezik vagy nem érhető el.");
        }
        else if (!category.IsCompatibleWith(TransactionType.Expense))
        {
            errors.Add("categoryId", "Költségkeret csak kiadási kategóriára adható meg.");
        }

        if (errors.Any)
        {
            return (null, errors.ToError());
        }

        var normalizedStart = Budget.NormalizeStart(command.PeriodType, command.StartDate);
        var siblings = await budgets.GetForCategoryAsync(userId, command.CategoryId, currency);
        var overlapping = siblings.Any(b =>
            b.Id != excludeBudgetId && BudgetPeriods.Overlaps(normalizedStart, command.EndDate, b.StartDate, b.EndDate));
        if (overlapping)
        {
            return (null, Error.Conflict(
                "Erre a kategóriára és pénznemre már van költségkeret az adott időszakban. Módosítsd a meglévőt, vagy válassz másik időszakot."));
        }

        return (command with { Currency = currency, StartDate = normalizedStart }, null);
    }
}
