using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>A tanításra használható minták forrása (kézzel kategorizált tranzakciók rendszerkategóriákkal).</summary>
public interface ITrainingDataSource
{
    Task<IReadOnlyList<TrainingSample>> GetSamplesAsync(CancellationToken cancellationToken = default);
}

/// <summary>A tényleges ML.NET tanítás. Az Application nem függ az ML.NET-től; csak ezt a vékony határt látja.</summary>
public interface IMlModelTrainer
{
    /// <summary>Betanít egy modellt, és elmenti a modelltárba a megadott fájlnéven.</summary>
    void Train(IReadOnlyList<MlTrainingExample> examples, string modelFileName);
}

/// <summary>Betöltött modellek futtatása.</summary>
public interface IMlModelRuntime
{
    /// <summary>Predikció a megadott modellfájllal. Hiányzó vagy használhatatlan modellnél <see cref="MlModelException"/>.</summary>
    MlPrediction Predict(string modelFileName, string features);
}

/// <summary>A szerver által felügyelt modelltár. A fájlnevet mindig a szerver generálja.</summary>
public interface IMlModelStore
{
    string NewFileName(string version);

    bool Exists(string fileName);

    void Delete(string fileName);
}

public interface IMlModelRepository
{
    Task<MlModelVersion?> GetActiveAsync();

    Task<MlModelVersion?> GetByIdAsync(Guid id);

    /// <summary>Az összes verzió, a legújabb elöl.</summary>
    Task<IReadOnlyList<MlModelVersion>> GetAllAsync();

    Task AddAsync(MlModelVersion version);

    /// <summary>Az előző aktív verzió deaktiválása és az új aktiválása egyetlen tranzakcióban (egyszerre csak egy aktív modell lehet).</summary>
    Task ActivateAsync(MlModelVersion version, DateTime activatedAtUtc);

    Task DeactivateAsync(MlModelVersion version);
}

/// <summary>UC-17: a kézi javítások naplója.</summary>
public interface ICategorizationFeedbackRepository
{
    /// <summary>A javított tranzakció és a hozzá tartozó visszajelzés mentése egyetlen lépésben (vagy mindkettő, vagy egyik sem).</summary>
    Task SaveCorrectionAsync(Transaction transaction, CategorizationFeedback feedback);
}
