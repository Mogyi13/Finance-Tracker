using FinanceTracker.Application.Features.Categorization.Ml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace FinanceTracker.Infrastructure.MachineLearning;

/// <summary>
/// UC-18 / UC-20: a kategorizáló modell tanítása ML.NET-tel. Szöveges jellemzők (szó- és karakter-n-gramok,
/// <c>FeaturizeText</c>) fölött többosztályos maximum entrópia (SDCA) osztályozó; a kimenet softmax valószínűség,
/// amelynek a maximuma a predikció megbízhatósága (UC-19). A bemeneti szöveg már normalizált (<see cref="FeatureText"/>).
/// </summary>
public sealed class MlNetModelTrainer(MlModelStore store, MlSettings settings, ILogger<MlNetModelTrainer> logger) : IMlModelTrainer
{
    public void Train(IReadOnlyList<MlTrainingExample> examples, string modelFileName)
    {
        var path = store.Resolve(modelFileName);

        try
        {
            store.EnsureDirectory();

            var ml = new MLContext(seed: settings.Seed);
            var data = ml.Data.LoadFromEnumerable(examples.Select(e => new ModelInput
            {
                Text = e.Features,
                Label = e.CategoryId.ToString("N")
            }));

            var pipeline = ml.Transforms.Conversion.MapValueToKey("Label")
                .Append(ml.Transforms.Text.FeaturizeText("Features", "Text"))
                .Append(ml.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(data);
            ml.Model.Save(model, data.Schema, path);

            logger.LogInformation("Modell betanítva: {File} ({Count} példa).", modelFileName, examples.Count);
        }
        catch (Exception ex) when (ex is not MlModelException)
        {
            throw new MlModelException("A tanítás közben hiba történt: " + ex.Message, ex);
        }
    }
}
