using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.CorrectCategory;

/// <summary>
/// UC-17: kézi kategóriajavítás. A választott kategóriát a háttérrendszer is ellenőrzi, a tranzakció ManuallyAssigned
/// állapotú lesz (a kézi döntés felülírja az automatikus javaslatot), és a javítás visszajelzésként megmarad a későbbi
/// modellértékeléshez. A visszajelzés mentése nem indít újratanítást.
/// </summary>
public sealed class CorrectCategoryHandler(
    ICurrentUser currentUser,
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ICategorizationFeedbackRepository feedbacks,
    IDerivedDataInvalidator derivedData,
    TimeProvider timeProvider)
{
    public async Task<Result<TransactionDto>> HandleAsync(CorrectCategoryCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var errors = new FieldErrors();
        if (command.TransactionId == Guid.Empty)
        {
            errors.Add("transactionId", "A tranzakció megadása kötelező.");
        }

        if (command.CategoryId == Guid.Empty)
        {
            errors.Add("categoryId", "A kategória megadása kötelező.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        // A tulajdonos ellenőrzése: más felhasználó tranzakciója ugyanúgy "nem található".
        var transaction = await transactions.GetByIdAsync(userId, command.TransactionId);
        if (transaction is null)
        {
            return Error.NotFound("A tranzakció nem található.");
        }

        var category = await categories.GetByIdAsync(command.CategoryId);
        TransactionFieldRules.ValidateCategory(category, userId, transaction.Type, errors);

        if (command.PredictedCategoryId is { } predictedId)
        {
            var predicted = predictedId == Guid.Empty ? null : await categories.GetByIdAsync(predictedId);
            if (predicted is null || !predicted.IsAvailableTo(userId))
            {
                errors.Add("predictedCategoryId", "A javasolt kategória nem létezik vagy nem érhető el.");
            }
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        // Már kézzel beállított, azonos kategória: nincs változás, ezért nincs új visszajelzés sem (a kérés ismételhető).
        if (transaction.CategoryId == category!.Id && transaction.CategorizationStatus == CategorizationStatus.ManuallyAssigned)
        {
            return Result<TransactionDto>.Ok(TransactionDto.From(transaction, category.Name));
        }

        var predictedCategoryId = command.PredictedCategoryId
            ?? (transaction.CategorizationStatus is CategorizationStatus.RuleBased or CategorizationStatus.MachineLearning
                ? transaction.CategoryId
                : null);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        transaction.CorrectCategory(category, now);
        var feedback = CategorizationFeedback.Create(Guid.NewGuid(), transaction.Id, predictedCategoryId, category.Id, now);

        await feedbacks.SaveCorrectionAsync(transaction, feedback);

        await derivedData.InvalidateAsync(userId, new[] { transaction.TransactionDate });

        return Result<TransactionDto>.Ok(TransactionDto.From(transaction, category.Name));
    }
}
