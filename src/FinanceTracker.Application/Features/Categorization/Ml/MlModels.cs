using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>Egy tanításra használható, kézzel kategorizált tranzakció. A tulajdonos azonosítója csak az alapmegoldás kiértékeléséhez kell, a tanítóhoz nem jut el.</summary>
public sealed record TrainingSample(Guid TransactionId, Guid OwnerId, TransactionType Type, string? Description, Guid CategoryId);

/// <summary>A tanítónak átadott példa: normalizált szöveg és a helyes kategória. Személyes azonosítót nem tartalmaz.</summary>
public sealed record MlTrainingExample(string Features, Guid CategoryId);

/// <summary>A modell predikciója: a legvalószínűbb kategória és a megbízhatósága (0 és 1 között).</summary>
public sealed record MlPrediction(Guid CategoryId, double Confidence);

/// <summary>A modell nem tölthető be vagy nem használható (hiányzó vagy sérült fájl, hibás kimenet).</summary>
public sealed class MlModelException : Exception
{
    public MlModelException(string message)
        : base(message)
    {
    }

    public MlModelException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public enum ConfidenceLevel
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum CategorizationDecision
{
    /// <summary>Nincs használható javaslat: a tranzakció kategorizálatlan marad.</summary>
    None = 0,

    /// <summary>A javaslat megjelenik, de a felhasználónak kell megerősítenie.</summary>
    Suggest = 1,

    /// <summary>A kategória automatikusan hozzárendelhető.</summary>
    AutoAssign = 2
}

public enum PredictionSource
{
    None = 0,
    Rule = 1,
    Model = 2
}
