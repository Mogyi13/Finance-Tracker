using FinanceTracker.Application.Features.Categorization.Training;

namespace FinanceTracker.Web.Services;

/// <summary>Az API üzemeltetői modellkezelő végpontjainak kliense (UC-20).</summary>
public sealed class ModelAdminApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    private const string BaseUrl = "api/categorization/models";

    /// <summary>Sikeres, ha a bejelentkezett felhasználó üzemeltető (egyébként 403).</summary>
    public Task<ApiResult<bool>> HasAccessAsync()
        => SendNoContentAsync(HttpMethod.Get, $"{BaseUrl}/access");

    public Task<ApiResult<List<ModelVersionDto>>> GetAllAsync()
        => SendAsync<List<ModelVersionDto>>(HttpMethod.Get, BaseUrl);

    public Task<ApiResult<ModelVersionDetailsDto>> GetAsync(Guid id)
        => SendAsync<ModelVersionDetailsDto>(HttpMethod.Get, $"{BaseUrl}/{id}");

    public Task<ApiResult<ModelVersionDto>> TrainAsync()
        => SendAsync<ModelVersionDto>(HttpMethod.Post, $"{BaseUrl}/train");

    public Task<ApiResult<ModelVersionDto>> ActivateAsync(Guid id)
        => SendAsync<ModelVersionDto>(HttpMethod.Post, $"{BaseUrl}/{id}/activate");

    public Task<ApiResult<ModelVersionDto>> DeactivateAsync(Guid id)
        => SendAsync<ModelVersionDto>(HttpMethod.Post, $"{BaseUrl}/{id}/deactivate");
}
