using FinanceTracker.Application.Features.Transactions;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.DerivedData;

/// <summary>
/// Ideiglenes megvalósítás: egyelőre nincsenek tárolt elemzési eredmények, ezért csak naplóz.
/// Az elemzések és előrejelzések bevezetésekor ez jelöli majd újraszámításra az érintett időszakokat.
/// </summary>
public sealed class LoggingDerivedDataInvalidator(ILogger<LoggingDerivedDataInvalidator> logger) : IDerivedDataInvalidator
{
    public Task InvalidateAsync(Guid userId, IReadOnlyCollection<DateOnly> affectedDates)
    {
        logger.LogInformation(
            "Származtatott adatok érvénytelenítése: felhasználó {UserId}, érintett dátumok: {Dates}",
            userId,
            string.Join(", ", affectedDates));

        return Task.CompletedTask;
    }
}
