using FinanceTracker.Application.Features.Categories;
using FinanceTracker.Application.Features.Categories.CreateCategory;
using FinanceTracker.Application.Features.Categories.UpdateCategory;

namespace FinanceTracker.Web.Services;

/// <summary>Az API kategóriavégpontjainak kliense (UC-10).</summary>
public sealed class CategoryApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    public Task<ApiResult<List<CategoryDto>>> GetAllAsync()
        => SendAsync<List<CategoryDto>>(HttpMethod.Get, "api/categories");

    public Task<ApiResult<CategoryDto>> CreateAsync(CreateCategoryCommand command)
        => SendAsync<CategoryDto>(HttpMethod.Post, "api/categories", command);

    public Task<ApiResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryCommand command)
        => SendAsync<CategoryDto>(HttpMethod.Put, $"api/categories/{id}", command);

    public Task<ApiResult<bool>> DeleteAsync(Guid id)
        => SendNoContentAsync(HttpMethod.Delete, $"api/categories/{id}");
}
