using FinanceTracker.Application.Features.Authentication.Login;
using FinanceTracker.Application.Features.Authentication.Register;
using FinanceTracker.Application.Features.Profile;
using FinanceTracker.Application.Features.Profile.UpdateProfile;

namespace FinanceTracker.Web.Services;

/// <summary>Az API auth végpontjainak kliense.</summary>
public sealed class AuthApiClient(HttpClient http, JwtAuthenticationStateProvider session) : ApiClientBase(http, session)
{
    public Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterCommand command)
        => SendAsync<RegisterResponse>(HttpMethod.Post, "api/auth/register", command, authorize: false);

    public Task<ApiResult<LoginResponse>> LoginAsync(LoginCommand command)
        => SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", command, authorize: false);

    public Task<ApiResult<UserProfileDto>> GetMeAsync()
        => SendAsync<UserProfileDto>(HttpMethod.Get, "api/auth/me");

    public Task<ApiResult<UserProfileDto>> UpdateProfileAsync(UpdateProfileCommand command)
        => SendAsync<UserProfileDto>(HttpMethod.Put, "api/auth/profile", command);
}
