using System.Globalization;
using FinanceTracker.Application.Features.Analytics.ComparePeriods;
using FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;
using FinanceTracker.Application.Features.Analytics.GetOverview;
using FinanceTracker.Application.Features.Analytics.Recurring;

namespace FinanceTracker.Web.Services;

/// <summary>Az API elemzési végpontjainak kliense (UC-21–UC-24).</summary>
public sealed class AnalyticsApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/analytics";

    public Task<ApiResult<FinancialOverviewDto>> GetOverviewAsync(DateOnly from, DateOnly to)
        => SendAsync<FinancialOverviewDto>(HttpMethod.Get, $"{BaseUrl}/overview?{Period("from", from, "to", to)}");

    public Task<ApiResult<CategoryBreakdownDto>> GetCategoryBreakdownAsync(DateOnly from, DateOnly to)
        => SendAsync<CategoryBreakdownDto>(HttpMethod.Get, $"{BaseUrl}/categories?{Period("from", from, "to", to)}");

    public Task<ApiResult<PeriodComparisonDto>> CompareAsync(DateRange basePeriod, DateRange comparedPeriod)
        => SendAsync<PeriodComparisonDto>(
            HttpMethod.Get,
            $"{BaseUrl}/compare?{Period("baseFrom", basePeriod.From, "baseTo", basePeriod.To)}&{Period("comparedFrom", comparedPeriod.From, "comparedTo", comparedPeriod.To)}");

    public Task<ApiResult<RecurringTransactionsDto>> GetRecurringAsync()
        => SendAsync<RecurringTransactionsDto>(HttpMethod.Get, $"{BaseUrl}/recurring");

    public Task<ApiResult<RecurringTransactionsDto>> AnalyzeRecurringAsync()
        => SendAsync<RecurringTransactionsDto>(HttpMethod.Post, $"{BaseUrl}/recurring/analyze");

    /// <summary>A dátumok kultúrafüggetlen (ISO) formátumban kerülnek a lekérdezésbe.</summary>
    internal static string Period(string fromName, DateOnly from, string toName, DateOnly to)
        => $"{fromName}={from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&{toName}={to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
}
