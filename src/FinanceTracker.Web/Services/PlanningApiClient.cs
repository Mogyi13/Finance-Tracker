using System.Globalization;
using FinanceTracker.Application.Features.Anomalies;
using FinanceTracker.Application.Features.Budgets;
using FinanceTracker.Application.Features.Forecasting;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>Az API anomáliavégpontjainak kliense (UC-25).</summary>
public sealed class AnomalyApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/analytics/anomalies";

    public Task<ApiResult<AnomaliesDto>> GetAsync(bool includeReviewed)
        => SendAsync<AnomaliesDto>(HttpMethod.Get, $"{BaseUrl}?includeReviewed={(includeReviewed ? "true" : "false")}");

    public Task<ApiResult<AnomalyAnalysisDto>> AnalyzeAsync(double? threshold = null)
        => SendAsync<AnomalyAnalysisDto>(HttpMethod.Post,
            threshold is { } t ? $"{BaseUrl}/analyze?threshold={t.ToString("0.0#", CultureInfo.InvariantCulture)}" : $"{BaseUrl}/analyze");

    public Task<ApiResult<bool>> ReviewAsync(Guid id)
        => SendNoContentAsync(HttpMethod.Post, $"{BaseUrl}/{id}/review");
}

/// <summary>Az API költségkeret-végpontjainak kliense (UC-26, UC-27).</summary>
public sealed class BudgetApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/budgets";

    public Task<ApiResult<List<BudgetDto>>> GetAllAsync()
        => SendAsync<List<BudgetDto>>(HttpMethod.Get, BaseUrl);

    public Task<ApiResult<BudgetStatusesDto>> GetStatusAsync()
        => SendAsync<BudgetStatusesDto>(HttpMethod.Get, $"{BaseUrl}/status");

    public Task<ApiResult<BudgetDto>> CreateAsync(SaveBudgetCommand command)
        => SendAsync<BudgetDto>(HttpMethod.Post, BaseUrl, command);

    public Task<ApiResult<BudgetDto>> UpdateAsync(Guid id, SaveBudgetCommand command)
        => SendAsync<BudgetDto>(HttpMethod.Put, $"{BaseUrl}/{id}", command);

    public Task<ApiResult<bool>> DeleteAsync(Guid id)
        => SendNoContentAsync(HttpMethod.Delete, $"{BaseUrl}/{id}");
}

/// <summary>Az API előrejelzési végpontjainak kliense (UC-28).</summary>
public sealed class ForecastApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/forecasting";

    public Task<ApiResult<ForecastDto>> GetAsync(ForecastType type)
        => SendAsync<ForecastDto>(HttpMethod.Get, type == ForecastType.MonthlyExpenses ? $"{BaseUrl}/monthly" : $"{BaseUrl}/cashflow");

    public Task<ApiResult<ForecastDto>> GenerateAsync(ForecastType type)
        => SendAsync<ForecastDto>(HttpMethod.Post, $"{BaseUrl}/generate?type={type}");

    public Task<ApiResult<ForecastEvaluationDto>> EvaluateAsync(ForecastType type)
        => SendAsync<ForecastEvaluationDto>(HttpMethod.Get, $"{BaseUrl}/evaluation?type={type}");
}
