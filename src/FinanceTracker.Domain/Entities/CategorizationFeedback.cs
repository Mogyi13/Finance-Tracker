using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// UC-17: a felhasználó kézi kategóriajavításának (vagy egy javaslat megerősítésének) naplója.
/// A későbbi modellértékeléshez és tanításhoz megőrzött adat; a rögzítése önmagában nem indít újratanítást.
/// </summary>
public sealed class CategorizationFeedback
{
    // Az EF Core számára.
    private CategorizationFeedback()
    {
    }

    public Guid Id { get; private set; }

    public Guid TransactionId { get; private set; }

    /// <summary>Az automatikusan (szabály vagy modell) javasolt kategória; null, ha nem volt előzetes javaslat.</summary>
    public Guid? PredictedCategoryId { get; private set; }

    /// <summary>A felhasználó által választott (helyes) kategória.</summary>
    public Guid CorrectedCategoryId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static CategorizationFeedback Create(
        Guid id,
        Guid transactionId,
        Guid? predictedCategoryId,
        Guid correctedCategoryId,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A visszajelzés azonosítója nem lehet üres.");
        }

        if (transactionId == Guid.Empty)
        {
            throw new DomainException("A visszajelzéshez tranzakció kell.");
        }

        if (correctedCategoryId == Guid.Empty)
        {
            throw new DomainException("A visszajelzéshez javított kategória kell.");
        }

        if (predictedCategoryId == Guid.Empty)
        {
            throw new DomainException("A javasolt kategória azonosítója érvénytelen.");
        }

        return new CategorizationFeedback
        {
            Id = id,
            TransactionId = transactionId,
            PredictedCategoryId = predictedCategoryId,
            CorrectedCategoryId = correctedCategoryId,
            CreatedAt = createdAtUtc
        };
    }

    /// <summary>Egyezett-e a javaslat a végső választással (a javaslatot a felhasználó megerősítette).</summary>
    public bool IsConfirmation => PredictedCategoryId == CorrectedCategoryId;
}
