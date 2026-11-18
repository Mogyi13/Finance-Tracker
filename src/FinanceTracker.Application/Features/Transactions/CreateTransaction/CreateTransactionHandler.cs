using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Transactions.CreateTransaction;

/// <summary>UC-05: tranzakció rögzítése.</summary>
public sealed class CreateTransactionHandler(
    ICurrentUser currentUser,
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ITransactionCategorizer categorizer,
    TimeProvider timeProvider)
{
    public async Task<Result<TransactionDto>> HandleAsync(CreateTransactionCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var currency = TransactionFieldRules.NormalizeCurrency(command.Currency);
        var description = TransactionFieldRules.NormalizeDescription(command.Description);

        var errors = new FieldErrors();
        TransactionFieldRules.Validate(command.Type, command.Amount, currency, description, command.TransactionDate, errors);

        Category? selectedCategory = null;
        if (command.CategoryId is { } categoryId)
        {
            selectedCategory = await categories.GetByIdAsync(categoryId);
            TransactionFieldRules.ValidateCategory(selectedCategory, userId, command.Type, errors);
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var details = TransactionDetails.Create(command.Type, command.Amount, currency, description, command.TransactionDate);
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            userId,
            details,
            TransactionSource.Manual,
            timeProvider.GetUtcNow().UtcDateTime);

        var assignedCategory = selectedCategory;
        if (selectedCategory is not null)
        {
            transaction.AssignCategory(selectedCategory, CategorizationStatus.ManuallyAssigned);
        }
        else
        {
            assignedCategory = await TryCategorizeAutomaticallyAsync(transaction, userId);
        }

        await transactions.AddAsync(transaction);

        return Result<TransactionDto>.Ok(TransactionDto.From(transaction, assignedCategory?.Name));
    }

    /// <summary>
    /// Automatikus kategorizálás, ha a felhasználó nem választott kategóriát. Ha nincs használható
    /// javaslat, a tranzakció kategorizálatlan marad.
    /// </summary>
    private async Task<Category?> TryCategorizeAutomaticallyAsync(Transaction transaction, Guid userId)
    {
        var suggestion = await categorizer.SuggestAsync(userId, transaction.Type, transaction.Description);
        if (suggestion is null)
        {
            return null;
        }

        var category = await categories.GetByIdAsync(suggestion.CategoryId);
        if (category is null
            || !category.IsAvailableTo(userId)
            || !category.IsCompatibleWith(transaction.Type)
            || suggestion.Status is CategorizationStatus.Uncategorized)
        {
            return null;
        }

        transaction.AssignCategory(category, suggestion.Status);
        return category;
    }
}
