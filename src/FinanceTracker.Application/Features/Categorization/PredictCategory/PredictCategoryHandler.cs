using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Application.Features.Categorization.PredictCategory;

/// <summary>
/// UC-18 / UC-19: kategóriajavaslat egy leíráshoz. Nem módosít semmit; a saját szabályokat (UC-15) és az aktív
/// modellt a rögzítéskor használt sorrendben értékeli ki, és a megbízhatóság alapján megmondja, mi lenne a döntés.
/// </summary>
public sealed class PredictCategoryHandler(
    ICurrentUser currentUser,
    CategoryPredictionService predictions,
    ICategoryRepository categories)
{
    public async Task<Result<CategoryPredictionDto>> HandleAsync(PredictCategoryCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var errors = new FieldErrors();
        if (!Enum.IsDefined(command.Type))
        {
            errors.Add("type", "A tranzakció típusa érvénytelen.");
        }

        if (command.Description is { } description && description.Length > TransactionDetails.MaxDescriptionLength)
        {
            errors.Add("description", $"A leírás legfeljebb {TransactionDetails.MaxDescriptionLength} karakter lehet.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var prediction = await predictions.PredictAsync(userId, command.Type, command.Description);

        string? categoryName = null;
        if (prediction.CategoryId is { } categoryId)
        {
            categoryName = (await categories.GetByIdAsync(categoryId))?.Name;
        }

        return Result<CategoryPredictionDto>.Ok(CategoryPredictionDto.From(prediction, categoryName));
    }
}
