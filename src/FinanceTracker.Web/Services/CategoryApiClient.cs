using FinanceTracker.Application.Features.Categories;

namespace FinanceTracker.Web.Services;

public sealed class CategoryApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    public Task<ApiResult<List<CategoryDto>>> GetAllAsync()
        => SendAsync<List<CategoryDto>>(HttpMethod.Get, "api/categories");
}
