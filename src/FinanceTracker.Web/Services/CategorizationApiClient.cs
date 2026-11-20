using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.CorrectCategory;
using FinanceTracker.Application.Features.Categorization.GetSuggestions;
using FinanceTracker.Application.Features.Transactions;

namespace FinanceTracker.Web.Services;

/// <summary>Az API kategóriajavítási és javaslat végpontjainak kliense (UC-17, UC-19).</summary>
public sealed class CategorizationApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/categorization";

    public Task<ApiResult<SuggestionsDto>> GetSuggestionsAsync()
        => SendAsync<SuggestionsDto>(HttpMethod.Get, $"{BaseUrl}/suggestions");

    /// <summary>UC-17: kézi kategóriajavítás vagy javaslat megerősítése.</summary>
    public Task<ApiResult<TransactionDto>> SubmitFeedbackAsync(Guid transactionId, Guid categoryId, Guid? predictedCategoryId = null)
        => SendAsync<TransactionDto>(HttpMethod.Post, $"{BaseUrl}/feedback", new CorrectCategoryCommand(transactionId, categoryId, predictedCategoryId));
}
