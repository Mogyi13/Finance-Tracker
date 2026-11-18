namespace FinanceTracker.Application.Features.Transactions;

/// <summary>
/// A tranzakciók módosítása vagy törlése után a származtatott eredmények (elemzések, előrejelzések)
/// újraszámításra jelölésének bővítési pontja.
/// </summary>
public interface IDerivedDataInvalidator
{
    Task InvalidateAsync(Guid userId, IReadOnlyCollection<DateOnly> affectedDates);
}
