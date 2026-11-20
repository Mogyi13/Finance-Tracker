using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Application.Features.Transactions;
using System.Runtime;

namespace FinanceTracker.Application.Features.Categorization.GetSuggestions;

/// <summary>
/// UC-19: a kategorizálatlan tranzakciókhoz tartozó, megerősítésre váró javaslatok. A javaslatok menet közben számolódnak
/// (nem tárolódnak), így mindig az éppen aktív modellt és a mai szabályokat tükrözik. A megerősítés vagy javítás a UC-17 végponton történik.
/// </summary>
public sealed class GetSuggestionsHandler(
    ICurrentUser currentUser,
    ITransactionQueries queries,
    CategoryPredictionService predictions,
    ICategoryRepository categories,
    MlSettings settings)
{
    public async Task<Result<SuggestionsDto>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var uncategorized = await queries.GetUncategorizedAsync(userId, Math.Max(1, settings.MaxSuggestionScan));
        var names = (await categories.GetAvailableAsync(userId)).ToDictionary(c => c.Id, c => c.Name);

        var items = new List<SuggestionItemDto>();
        foreach (var transaction in uncategorized.Items)
        {
            var prediction = await predictions.PredictAsync(userId, transaction.Type, transaction.Description);
            if (prediction.CategoryId is not { } categoryId)
            {
                continue;
            }

            names.TryGetValue(categoryId, out var name);
            items.Add(new SuggestionItemDto(transaction, CategoryPredictionDto.From(prediction, name)));
        }

        return Result<SuggestionsDto>.Ok(new SuggestionsDto(uncategorized.TotalCount, uncategorized.Items.Count, items));
    }
}
