using FinanceTracker.Application.Features.Categorization.Ml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System.Collections.Concurrent;

namespace FinanceTracker.Infrastructure.MachineLearning;

/// <summary>
/// Betöltött ML.NET modellek futtatása. A modellek fájlnév szerint gyorsítótárazódnak (a fájlnév verziónként egyedi,
/// ezért új modell aktiválásakor nincs elavult gyorsítótár-bejegyzés). A <c>PredictionEngine</c> nem szálbiztos,
/// ezért modellenként zárral védett; a hobbi léptékű használathoz ez elegendő.
/// </summary>
public sealed class MlNetModelRuntime(MlModelStore store, ILogger<MlNetModelRuntime> logger) : IMlModelRuntime
{
    private readonly ConcurrentDictionary<string, Lazy<LoadedModel>> _models = new(StringComparer.Ordinal);

    public MlPrediction Predict(string modelFileName, string features)
    {
        LoadedModel loaded;
        try
        {
            loaded = _models.GetOrAdd(modelFileName, name => new Lazy<LoadedModel>(() => Load(name))).Value;
        }
        catch (MlModelException)
        {
            _models.TryRemove(modelFileName, out _);
            throw;
        }

        try
        {
            ModelOutput output;
            lock (loaded.Gate)
            {
                output = loaded.Engine.Predict(new ModelInput { Text = features });
            }

            if (!Guid.TryParseExact(output.PredictedLabel, "N", out var categoryId) || output.Score.Length == 0)
            {
                throw new MlModelException("A modell kimenete nem értelmezhető.");
            }

            return new MlPrediction(categoryId, output.Score.Max());
        }
        catch (Exception ex) when (ex is not MlModelException)
        {
            logger.LogError(ex, "A modell futtatása sikertelen: {File}", modelFileName);
            throw new MlModelException("A modell futtatása sikertelen.", ex);
        }
    }

    private LoadedModel Load(string modelFileName)
    {
        var path = store.Resolve(modelFileName);
        if (!File.Exists(path))
        {
            logger.LogError("A modellfájl nem található: {File}", modelFileName);
            throw new MlModelException("A modellfájl nem található.");
        }

        try
        {
            var ml = new MLContext();
            using var stream = File.OpenRead(path);
            var transformer = ml.Model.Load(stream, out _);
            return new LoadedModel(ml.Model.CreatePredictionEngine<ModelInput, ModelOutput>(transformer));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A modellfájl betöltése sikertelen: {File}", modelFileName);
            throw new MlModelException("A modellfájl betöltése sikertelen.", ex);
        }
    }

    private sealed class LoadedModel(PredictionEngine<ModelInput, ModelOutput> engine)
    {
        public PredictionEngine<ModelInput, ModelOutput> Engine { get; } = engine;

        public object Gate { get; } = new();
    }
}
