using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Transactions.UpdateTransaction;

/// <summary>UC-07: tranzakció módosítása.</summary>
public sealed class UpdateTransactionHandler(
    ICurrentUser currentUser,
    ITransactionRepository transactions,
    ICategoryRepository categories,
    IDerivedDataInvalidator derivedData,
    TimeProvider timeProvider)
{
    public async Task<Result<TransactionDto>> HandleAsync(Guid id, UpdateTransactionCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        // A tulajdonos ellenőrzése: más felhasználó tranzakciója ugyanúgy "nem található".
        var transaction = await transactions.GetByIdAsync(userId, id);
        if (transaction is null)
        {
            return Error.NotFound("A tranzakció nem található.");
        }

        var currency = TransactionFieldRules.NormalizeCurrency(command.Currency);
        var description = TransactionFieldRules.NormalizeDescription(command.Description);

        var errors = new FieldErrors();
        TransactionFieldRules.Validate(command.Type, command.Amount, currency, description, command.TransactionDate, errors);

        Category? category = null;
        if (command.CategoryId is { } categoryId)
        {
            category = await categories.GetByIdAsync(categoryId);
            TransactionFieldRules.ValidateCategory(category, userId, command.Type, errors);
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var previousDate = transaction.TransactionDate;

        var details = TransactionDetails.Create(command.Type, command.Amount, currency, description, command.TransactionDate);
        transaction.Update(details, category, timeProvider.GetUtcNow().UtcDateTime);

        await transactions.UpdateAsync(transaction);

        // A korábbi és az új dátum is érintett időszak, ha a dátum módosult.
        await derivedData.InvalidateAsync(userId, new[] { previousDate, transaction.TransactionDate }.Distinct().ToList());

        return Result<TransactionDto>.Ok(TransactionDto.From(transaction, category?.Name));
    }
}
