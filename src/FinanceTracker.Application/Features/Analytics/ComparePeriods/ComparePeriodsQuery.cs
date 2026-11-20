namespace FinanceTracker.Application.Features.Analytics.ComparePeriods;

/// <summary>
/// UC-23: két időszak (mindkét nap beletartozik). Az „alap” az összehasonlítás kiindulása, az „összehasonlított” az, amit hozzá mérünk;
/// az eltérések = összehasonlított − alap. A két időszak átfedhet, a hosszuk is különbözhet.
/// </summary>
public sealed record ComparePeriodsQuery(
    DateOnly? BaseFrom,
    DateOnly? BaseTo,
    DateOnly? ComparedFrom,
    DateOnly? ComparedTo);
