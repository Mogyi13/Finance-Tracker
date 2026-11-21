using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// UC-25: a rendszer által felismert szokatlan tranzakció. Az anomália statisztikai eltérést jelent, nem azt,
/// hogy a tranzakció hibás vagy csalárd (BR-11). A felhasználó áttekintettnek jelölheti.
/// </summary>
public sealed class FinancialAnomaly
{
    public const int MaxDescriptionLength = 500;

    // Az EF Core számára.
    private FinancialAnomaly()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid TransactionId { get; private set; }

    public AnomalyType AnomalyType { get; private set; }

    /// <summary>A számított pontszám (z-pontszám).</summary>
    public decimal? Score { get; private set; }

    /// <summary>Emberi nyelvű magyarázat arról, miért tekintjük szokatlannak.</summary>
    public string Description { get; private set; } = string.Empty;

    public DateTime DetectedAt { get; private set; }

    public bool IsReviewed { get; private set; }

    public static FinancialAnomaly Create(
        Guid id,
        Guid userId,
        Guid transactionId,
        AnomalyType type,
        decimal? score,
        string description,
        DateTime detectedAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty || transactionId == Guid.Empty)
        {
            throw new DomainException("Az anomáliának azonosító, tulajdonos és tranzakció kell.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Az anomália típusa érvénytelen.");
        }

        return new FinancialAnomaly
        {
            Id = id,
            UserId = userId,
            TransactionId = transactionId,
            AnomalyType = type,
            Score = score,
            Description = NormalizeDescription(description),
            DetectedAt = detectedAtUtc
        };
    }

    /// <summary>Újraelemzéskor a már ismert anomália adatainak frissítése; az áttekintett állapot megmarad.</summary>
    public void Refresh(decimal? score, string description, DateTime detectedAtUtc)
    {
        Score = score;
        Description = NormalizeDescription(description);
        DetectedAt = detectedAtUtc;
    }

    public void MarkReviewed() => IsReviewed = true;

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Az anomália magyarázata kötelező.");
        }

        var trimmed = description.Trim();
        return trimmed.Length > MaxDescriptionLength ? trimmed[..MaxDescriptionLength] : trimmed;
    }
}
