using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Egy betanított kategorizáló modell verziója és kiértékelésének eredménye. A modellfájl nem az adatbázisban,
/// hanem a szerver által felügyelt tárolóban van; a <see cref="ModelPath"/> csak a fájl neve (nem elérési út).
/// Egyszerre legfeljebb egy verzió aktív, és csak az aktiválási feltételeknek megfelelő verzió aktiválható.
/// </summary>
public sealed class MlModelVersion
{
    public const int MaxVersionLength = 50;
    public const int MaxModelPathLength = 500;

    // Az EF Core számára.
    private MlModelVersion()
    {
    }

    public Guid Id { get; private set; }

    public string Version { get; private set; } = string.Empty;

    /// <summary>A modellfájl neve a modelltárolóban (szerver által generált, felhasználói bemenetből nem származhat).</summary>
    public string ModelPath { get; private set; } = string.Empty;

    public decimal Accuracy { get; private set; }

    /// <summary>Makro átlagolt pontosság (precision).</summary>
    public decimal Precision { get; private set; }

    /// <summary>Makro átlagolt fedés (recall).</summary>
    public decimal Recall { get; private set; }

    public decimal F1Score { get; private set; }

    public bool IsActive { get; private set; }

    public int TrainingDataCount { get; private set; }

    public int TestDataCount { get; private set; }

    /// <summary>Egy predikció átlagos feldolgozási ideje milliszekundumban a tesztadatokon mérve.</summary>
    public decimal AveragePredictionMs { get; private set; }

    /// <summary>A szabályalapú alapmegoldás pontossága ugyanazon a tesztadaton (a nem illeszkedő tétel hibának számít).</summary>
    public decimal? BaselineAccuracy { get; private set; }

    /// <summary>Teljesítette-e a verzió az előre meghatározott aktiválási feltételeket.</summary>
    public bool MeetsCriteria { get; private set; }

    /// <summary>A teljes kiértékelési jelentés JSON-ban (osztályonkénti mutatók, küszöbelemzés, feltételek).</summary>
    public string? EvaluationJson { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ActivatedAt { get; private set; }

    public static MlModelVersion Create(
        Guid id,
        string version,
        string modelPath,
        decimal accuracy,
        decimal precision,
        decimal recall,
        decimal f1Score,
        int trainingDataCount,
        int testDataCount,
        decimal averagePredictionMs,
        decimal? baselineAccuracy,
        bool meetsCriteria,
        string? evaluationJson,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A modellverzió azonosítója nem lehet üres.");
        }

        if (string.IsNullOrWhiteSpace(version) || version.Length > MaxVersionLength)
        {
            throw new DomainException($"A verziócímke kötelező, és legfeljebb {MaxVersionLength} karakter lehet.");
        }

        if (!IsSafeFileName(modelPath))
        {
            throw new DomainException("A modellfájl neve érvénytelen.");
        }

        foreach (var metric in new[] { accuracy, precision, recall, f1Score })
        {
            if (metric is < 0m or > 1m)
            {
                throw new DomainException("A modell mutatói 0 és 1 közötti értékek lehetnek.");
            }
        }

        if (baselineAccuracy is < 0m or > 1m)
        {
            throw new DomainException("Az alapmegoldás pontossága 0 és 1 közötti érték lehet.");
        }

        if (trainingDataCount <= 0 || testDataCount <= 0)
        {
            throw new DomainException("A tanító és a tesztadatok száma pozitív kell legyen.");
        }

        if (averagePredictionMs < 0m)
        {
            throw new DomainException("Az átlagos predikciós idő nem lehet negatív.");
        }

        return new MlModelVersion
        {
            Id = id,
            Version = version.Trim(),
            ModelPath = modelPath,
            Accuracy = accuracy,
            Precision = precision,
            Recall = recall,
            F1Score = f1Score,
            TrainingDataCount = trainingDataCount,
            TestDataCount = testDataCount,
            AveragePredictionMs = averagePredictionMs,
            BaselineAccuracy = baselineAccuracy,
            MeetsCriteria = meetsCriteria,
            EvaluationJson = evaluationJson,
            CreatedAt = createdAtUtc,
            IsActive = false
        };
    }

    /// <summary>Aktiválás. Csak az aktiválási feltételeknek megfelelő verzió aktiválható; az előző aktív verzió deaktiválásáról a hívó gondoskodik.</summary>
    public void Activate(DateTime activatedAtUtc)
    {
        if (!MeetsCriteria)
        {
            throw new DomainException("A modellverzió nem teljesíti az aktiválási feltételeket, ezért nem aktiválható.");
        }

        IsActive = true;
        ActivatedAt = activatedAtUtc;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Csak egyszerű fájlnév fogadható el: nem lehet benne könyvtárelválasztó, "..", vagy vezérlő karakter.</summary>
    public static bool IsSafeFileName(string? name)
        => !string.IsNullOrWhiteSpace(name)
           && name.Length <= MaxModelPathLength
           && name == name.Trim()
           && !name.Contains("..", StringComparison.Ordinal)
           && name.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.');
}
