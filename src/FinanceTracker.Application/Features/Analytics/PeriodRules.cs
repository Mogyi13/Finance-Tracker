using FinanceTracker.Application.Common;

namespace FinanceTracker.Application.Features.Analytics;

/// <summary>Az elemzési időszakok közös ellenőrzése és alapértelmezése.</summary>
public static class PeriodRules
{
    /// <summary>Egy időszak legfeljebb ennyi napot fedhet le (nagyjából 10 év), hogy egy kérés ne terhelje túl az adatbázist.</summary>
    public const int MaxSpanDays = 3660;

    /// <summary>Az időszak ellenőrzése; a hibákat a megadott mezőnevekkel adja vissza.</summary>
    public static void Validate(FieldErrors errors, DateOnly? from, DateOnly? to, string fromField, string toField)
    {
        if (from is null)
        {
            errors.Add(fromField, "A kezdő dátum megadása kötelező.");
        }

        if (to is null)
        {
            errors.Add(toField, "A záró dátum megadása kötelező.");
        }

        if (from is not { } f || to is not { } t)
        {
            return;
        }

        if (f > t)
        {
            errors.Add(toField, "A záró dátum nem lehet korábbi a kezdő dátumnál.");
        }
        else if (t.DayNumber - f.DayNumber + 1 > MaxSpanDays)
        {
            errors.Add(toField, $"Az időszak legfeljebb {MaxSpanDays} napos lehet.");
        }
    }

    /// <summary>Az aktuális naptári hónap első és utolsó napja.</summary>
    public static (DateOnly From, DateOnly To) CurrentMonth(DateOnly today)
    {
        var first = new DateOnly(today.Year, today.Month, 1);
        return (first, first.AddMonths(1).AddDays(-1));
    }
}
