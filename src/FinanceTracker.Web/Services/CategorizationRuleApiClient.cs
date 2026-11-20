using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.CreateRule;
using FinanceTracker.Application.Features.Categorization.SetRuleActive;
using FinanceTracker.Application.Features.Categorization.SetRulePriority;
using FinanceTracker.Application.Features.Categorization.UpdateRule;

namespace FinanceTracker.Web.Services;

/// <summary>Az API kategorizálási szabály végpontjainak kliense (UC-16).</summary>
public sealed class CategorizationRuleApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/categorization/rules";

    public Task<ApiResult<List<CategorizationRuleDto>>> GetAllAsync()
        => SendAsync<List<CategorizationRuleDto>>(HttpMethod.Get, BaseUrl);

    public Task<ApiResult<CategorizationRuleDto>> CreateAsync(CreateRuleCommand command)
        => SendAsync<CategorizationRuleDto>(HttpMethod.Post, BaseUrl, command);

    public Task<ApiResult<CategorizationRuleDto>> UpdateAsync(Guid id, UpdateRuleCommand command)
        => SendAsync<CategorizationRuleDto>(HttpMethod.Put, $"{BaseUrl}/{id}", command);

    public Task<ApiResult<CategorizationRuleDto>> SetActiveAsync(Guid id, bool isActive)
        => SendAsync<CategorizationRuleDto>(HttpMethod.Patch, $"{BaseUrl}/{id}/active", new SetRuleActiveCommand(isActive));

    public Task<ApiResult<CategorizationRuleDto>> SetPriorityAsync(Guid id, int priority)
        => SendAsync<CategorizationRuleDto>(HttpMethod.Patch, $"{BaseUrl}/{id}/priority", new SetRulePriorityCommand(priority));

    public Task<ApiResult<bool>> DeleteAsync(Guid id)
        => SendNoContentAsync(HttpMethod.Delete, $"{BaseUrl}/{id}");
}
