using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinanceTracker.Application.Features.Authentication.Login;
using FinanceTracker.Application.Features.Authentication.Register;
using FinanceTracker.Application.Features.Profile;
using FinanceTracker.Application.Features.Profile.UpdateProfile;

namespace FinanceTracker.Web.Services;

/// <summary>Az API auth végpontjainak kliense. A Web szerver hívja az API-t, a böngésző nem.</summary>
public sealed class AuthApiClient(HttpClient http, JwtAuthenticationStateProvider session)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterCommand command)
        => SendAsync<RegisterResponse>(HttpMethod.Post, "api/auth/register", command, authorize: false);

    public Task<ApiResult<LoginResponse>> LoginAsync(LoginCommand command)
        => SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", command, authorize: false);

    public Task<ApiResult<UserProfileDto>> GetMeAsync()
        => SendAsync<UserProfileDto>(HttpMethod.Get, "api/auth/me", body: null, authorize: true);

    public Task<ApiResult<UserProfileDto>> UpdateProfileAsync(UpdateProfileCommand command)
        => SendAsync<UserProfileDto>(HttpMethod.Put, "api/auth/profile", command, authorize: true);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, bool authorize)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body, body.GetType(), options: Json);
            }

            if (authorize)
            {
                var token = await session.GetAccessTokenAsync();
                if (token is null)
                {
                    return ApiResult<T>.Failure(HttpStatusCode.Unauthorized, "A munkamenet lejárt, jelentkezz be újra.");
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<T>(Json);
                return value is null
                    ? ApiResult<T>.Failure(response.StatusCode, "Az API üres választ adott.")
                    : ApiResult<T>.Success(value, response.StatusCode);
            }

            return await ReadErrorAsync<T>(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Failure(null, "A szerver jelenleg nem érhető el. Próbáld újra később.");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure(null, "A szerver nem válaszolt időben. Próbáld újra később.");
        }
    }

    private static async Task<ApiResult<T>> ReadErrorAsync<T>(HttpResponseMessage response)
    {
        ProblemResponse? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(Json);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Nem ProblemDetails a válasz, általános üzenetet adunk.
        }

        var fallback = response.StatusCode == HttpStatusCode.Unauthorized
            ? "A művelethez bejelentkezés szükséges."
            : "Ismeretlen hiba történt.";
        var message = problem?.Detail ?? problem?.Title ?? fallback;

        return ApiResult<T>.Failure(response.StatusCode, message, problem?.Errors);
    }

    private sealed class ProblemResponse
    {
        public string? Title { get; set; }

        public string? Detail { get; set; }

        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
