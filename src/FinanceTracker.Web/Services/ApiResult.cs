using System.Net;

namespace FinanceTracker.Web.Services;

public sealed class ApiResult<T>
{
    private ApiResult() { }

    public bool IsSuccess { get; private init; }

    public T? Value { get; private init; }

    public HttpStatusCode? Status { get; private init; }

    public string? Error { get; private init; }

    public IReadOnlyDictionary<string, string[]> FieldErrors { get; private init; } = new Dictionary<string, string[]>();

    public bool IsUnauthorized => Status == HttpStatusCode.Unauthorized;

    /// <summary>A felhasználónak megjeleníthető hibaüzenetek.</summary>
    public IReadOnlyList<string> Messages
    {
        get
        {
            if (FieldErrors.Count > 0)
            {
                return FieldErrors.Values.SelectMany(v => v).ToList();
            }

            return Error is null ? new List<string>() : new List<string> { Error };
        }
    }

    public static ApiResult<T> Success(T value, HttpStatusCode status)
        => new() { IsSuccess = true, Value = value, Status = status };

    public static ApiResult<T> Failure(HttpStatusCode? status, string error, IReadOnlyDictionary<string, string[]>? fieldErrors = null)
        => new()
        {
            IsSuccess = false,
            Status = status,
            Error = error,
            FieldErrors = fieldErrors ?? new Dictionary<string, string[]>()
        };
}
