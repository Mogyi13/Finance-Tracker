using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceTracker.Web.Services;

/// <summary>
/// Az API-kliensek közös alapja: Bearer token csatolása, JSON (a enumok névvel), ProblemDetails hibák feldolgozása.
/// A Web szerver hívja az API-t, a böngésző nem.
/// </summary>
public abstract class ApiClientBase(HttpClient http, JwtAuthenticationStateProvider session)
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null, bool authorize = true)
        => ExecuteAsync<T>(method, url, body, authorize, async response =>
        {
            var value = await response.Content.ReadFromJsonAsync<T>(Json);
            return value is null
                ? ApiResult<T>.Failure(response.StatusCode, "Az API üres választ adott.")
                : ApiResult<T>.Success(value, response.StatusCode);
        });

    /// <summary>Olyan hívás, amelynek sikeres válasza nem tartalmaz törzset (például 204 törlésnél).</summary>
    protected Task<ApiResult<bool>> SendNoContentAsync(HttpMethod method, string url, bool authorize = true)
        => ExecuteAsync<bool>(method, url, null, authorize,
            response => Task.FromResult(ApiResult<bool>.Success(true, response.StatusCode)));

    private async Task<ApiResult<T>> ExecuteAsync<T>(
        HttpMethod method,
        string url,
        object? body,
        bool authorize,
        Func<HttpResponseMessage, Task<ApiResult<T>>> onSuccess)
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

            return response.IsSuccessStatusCode
                ? await onSuccess(response)
                : await ReadErrorAsync<T>(response);
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

        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "A művelethez bejelentkezés szükséges.",
            HttpStatusCode.NotFound => "A kért elem nem található.",
            _ => "Ismeretlen hiba történt."
        };
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
