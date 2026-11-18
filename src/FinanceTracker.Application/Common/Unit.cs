namespace FinanceTracker.Application.Common;

/// <summary>Érték nélküli sikeres eredmény (például törlésnél).</summary>
public readonly struct Unit
{
    public static readonly Unit Value = default;
}
