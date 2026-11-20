namespace FinanceTracker.Application.Common;

public enum ErrorKind
{
    Validation,
    Unauthorized,
    Locked,
    NotFound,
    Forbidden,
    Conflict,
    Failure
}

/// <summary>Use case hiba. Validációs hibánál a mezőnkénti üzenetek a Fields-ben vannak.</summary>
public sealed record Error(ErrorKind Kind, string Message, IReadOnlyDictionary<string, string[]>? Fields = null)
{
    public static Error Validation(IReadOnlyDictionary<string, string[]> fields, string message = "A megadott adatok érvénytelenek.")
        => new(ErrorKind.Validation, message, fields);

    public static Error Unauthorized(string message) => new(ErrorKind.Unauthorized, message);

    public static Error Locked(string message) => new(ErrorKind.Locked, message);

    public static Error NotFound(string message) => new(ErrorKind.NotFound, message);

    public static Error Forbidden(string message) => new(ErrorKind.Forbidden, message);

    public static Error Conflict(string message) => new(ErrorKind.Conflict, message);

    public static Error Failure(string message) => new(ErrorKind.Failure, message);
}

/// <summary>Use case eredménye: vagy érték, vagy hiba.</summary>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T? value, Error? error)
    {
        _value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Sikertelen eredménynek nincs értéke.");

    public static Result<T> Ok(T value) => new(value, null);

    public static implicit operator Result<T>(Error error) => new(default, error);
}
