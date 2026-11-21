using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// UC-28: egy mentett előrejelzés. Becslés, nem garantált pénzügyi eredmény (BR-10). A különböző módszerek
/// eredményei külön rekordok, hogy később összehasonlíthatók legyenek.
/// </summary>
public sealed class ForecastResult
{
    // Az EF Core számára.
    private ForecastResult()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ForecastType ForecastType { get; private set; }

    public ForecastMethod Method { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public DateOnly PeriodStart { get; private set; }

    public DateOnly PeriodEnd { get; private set; }

    public decimal PredictedAmount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>A használt ML-modell verziója, ha releváns (a mozgóátlag és a lineáris regresszió nem használ tárolt modellt).</summary>
    public Guid? ModelVersionId { get; private set; }

    public static ForecastResult Create(
        Guid id,
        Guid userId,
        ForecastType type,
        ForecastMethod method,
        string currency,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal predictedAmount,
        DateTime createdAtUtc,
        Guid? modelVersionId = null)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Az előrejelzésnek azonosító és tulajdonos kell.");
        }

        if (!Enum.IsDefined(type) || !Enum.IsDefined(method))
        {
            throw new DomainException("Az előrejelzés típusa vagy módszere érvénytelen.");
        }

        if (currency is not { Length: 3 })
        {
            throw new DomainException("A pénznem háromkarakteres kód.");
        }

        if (periodStart > periodEnd)
        {
            throw new DomainException("Az időszak kezdete nem lehet későbbi a végénél.");
        }

        return new ForecastResult
        {
            Id = id,
            UserId = userId,
            ForecastType = type,
            Method = method,
            Currency = currency,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PredictedAmount = decimal.Round(predictedAmount, 2),
            CreatedAt = createdAtUtc,
            ModelVersionId = modelVersionId
        };
    }
}
