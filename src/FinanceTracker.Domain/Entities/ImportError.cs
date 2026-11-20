using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>Egy importált sor (vagy a teljes fájl) hibája. A 0. sor a fájl szintű hibát jelöli.</summary>
public sealed class ImportError
{
    public const int MaxErrorCodeLength = 50;
    public const int MaxErrorMessageLength = 500;

    /// <summary>A fájl egészére vonatkozó hiba sorszáma.</summary>
    public const int FileLevelRow = 0;

    // Az EF Core számára.
    private ImportError()
    {
    }

    public Guid Id { get; private set; }

    public Guid ImportBatchId { get; private set; }

    /// <summary>A hibás sor sorszáma a fájlban (a fejléc az 1. sor), vagy 0 fájl szintű hibánál.</summary>
    public int RowNumber { get; private set; }

    public string ErrorCode { get; private set; } = string.Empty;

    public string ErrorMessage { get; private set; } = string.Empty;

    public static ImportError Create(Guid id, Guid importBatchId, int rowNumber, string errorCode, string errorMessage)
    {
        if (id == Guid.Empty || importBatchId == Guid.Empty)
        {
            throw new DomainException("Az importhiba azonosítói nem lehetnek üresek.");
        }

        if (rowNumber < 0)
        {
            throw new DomainException("A sor sorszáma nem lehet negatív.");
        }

        if (string.IsNullOrWhiteSpace(errorCode) || string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new DomainException("A hibakód és a hibaüzenet kötelező.");
        }

        return new ImportError
        {
            Id = id,
            ImportBatchId = importBatchId,
            RowNumber = rowNumber,
            ErrorCode = Truncate(errorCode.Trim(), MaxErrorCodeLength),
            ErrorMessage = Truncate(errorMessage.Trim(), MaxErrorMessageLength)
        };
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
