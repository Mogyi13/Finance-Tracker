using FinanceTracker.Application.Features.Transactions;

namespace FinanceTracker.Application.Features.Categorization.GetSuggestions;

public sealed record SuggestionItemDto(TransactionDto Transaction, CategoryPredictionDto Prediction);

/// <summary>A kategorizálatlan tranzakciók közül azok, amelyekhez a rendszer javaslatot tud adni.</summary>
/// <param name="ScannedCount">Ennyi kategorizálatlan tranzakciót vizsgált meg a rendszer.</param>
/// <param name="UncategorizedCount">Összesen ennyi kategorizálatlan tranzakciód van.</param>
public sealed record SuggestionsDto(int UncategorizedCount, int ScannedCount, IReadOnlyList<SuggestionItemDto> Items);
