using System.Text.Json;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categorization.Training;

/// <summary>Egy modellverzió nyilvános adatai. A modellfájl helye nem szerepel benne (azt csak a szerver ismeri).</summary>
public sealed record ModelVersionDto(
    Guid Id,
    string Version,
    decimal Accuracy,
    decimal Precision,
    decimal Recall,
    decimal F1Score,
    bool IsActive,
    int TrainingDataCount,
    int TestDataCount,
    decimal AveragePredictionMs,
    decimal? BaselineAccuracy,
    bool MeetsCriteria,
    DateTime CreatedAt,
    DateTime? ActivatedAt)
{
    internal static ModelVersionDto From(MlModelVersion v)
        => new(
            v.Id, v.Version, v.Accuracy, v.Precision, v.Recall, v.F1Score, v.IsActive, v.TrainingDataCount,
            v.TestDataCount, v.AveragePredictionMs, v.BaselineAccuracy, v.MeetsCriteria, v.CreatedAt, v.ActivatedAt);
}

/// <summary>A modellverzió adatai a teljes kiértékelési jelentéssel (osztályonkénti mutatók, küszöbelemzés, feltételek).</summary>
public sealed record ModelVersionDetailsDto(ModelVersionDto Version, JsonElement? Evaluation)
{
    internal static ModelVersionDetailsDto From(MlModelVersion v)
    {
        JsonElement? evaluation = null;
        if (!string.IsNullOrWhiteSpace(v.EvaluationJson))
        {
            using var document = JsonDocument.Parse(v.EvaluationJson);
            evaluation = document.RootElement.Clone();
        }

        return new ModelVersionDetailsDto(ModelVersionDto.From(v), evaluation);
    }
}
