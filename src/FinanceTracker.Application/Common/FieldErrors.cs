using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinanceTracker.Application.Common;

/// <summary>Mezőnkénti validációs hibák gyűjtője.</summary>
public sealed class FieldErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool Any => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var list))
        {
            list = new List<string>();
            _errors[field] = list;
        }

        list.Add(message);
    }

    public Error ToError()
        => Error.Validation(_errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()));
}
