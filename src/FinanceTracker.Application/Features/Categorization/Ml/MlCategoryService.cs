using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using System.Runtime;

namespace FinanceTracker.Application.Features.Categorization.Ml;

public enum MlOutcome
{
    /// <summary>Nincs aktív, használható modell.</summary>
    NoModel = 0,

    /// <summary>A leírásból nem képezhető modellbemenet (üres vagy csak számokból/írásjelekből áll).</summary>
    NoText = 1,

    /// <summary>A modell érvénytelen vagy a tranzakció típusával összeegyeztethetetlen kategóriát adott: a predikció elvetve.</summary>
    Rejected = 2,

    Predicted = 3
}

public sealed record MlCategoryResult(
    MlOutcome Outcome,
    Guid? CategoryId = null,
    double Confidence = 0,
    ConfidenceLevel? Level = null,
    CategorizationDecision Decision = CategorizationDecision.None,
    string? ModelVersion = null);

/// <summary>
/// UC-18 és UC-19: ML-alapú kategorizálás. Az aktív modellt kéréssorozatonként egyszer tölti be az adatbázisból
/// (egy nagy import sem generál soronként lekérdezést), a predikciót az <see cref="IMlModelRuntime"/> végzi.
/// A választ érvényesíti (létező, elérhető, a tranzakció típusával egyező kategória), majd a megbízhatóság alapján dönt.
/// A modell hibája sosem akadályozza a tranzakció mentését: ilyenkor a tranzakció kategorizálatlan marad.
/// </summary>
public sealed class MlCategoryService(
    IMlModelRepository models,
    IMlModelRuntime runtime,
    ICategoryRepository categories,
    MlSettings settings)
{
    private bool _activeLoaded;
    private MlModelVersion? _active;
    private readonly Dictionary<Guid, IReadOnlyDictionary<Guid, Category>> _categoryCache = new();

    public async Task<MlCategoryResult> PredictAsync(Guid userId, TransactionType type, string? description)
    {
        var features = FeatureText.Build(type, description);
        if (features is null)
        {
            return new MlCategoryResult(MlOutcome.NoText);
        }

        var active = await GetActiveAsync();
        if (active is null)
        {
            return new MlCategoryResult(MlOutcome.NoModel);
        }

        MlPrediction prediction;
        try
        {
            prediction = runtime.Predict(active.ModelPath, features);
        }
        catch (MlModelException)
        {
            // A hibás vagy hiányzó modell miatt a kategorizálás kimarad, a mentés nem hiúsulhat meg.
            return new MlCategoryResult(MlOutcome.NoModel);
        }

        var available = await GetCategoriesAsync(userId);
        if (!available.TryGetValue(prediction.CategoryId, out var category)
            || !category.IsAvailableTo(userId)
            || !category.IsCompatibleWith(type)
            || double.IsNaN(prediction.Confidence))
        {
            return new MlCategoryResult(MlOutcome.Rejected, ModelVersion: active.Version);
        }

        var confidence = Math.Clamp(prediction.Confidence, 0, 1);
        return new MlCategoryResult(
            MlOutcome.Predicted,
            category.Id,
            confidence,
            ConfidencePolicy.LevelOf(confidence, settings),
            ConfidencePolicy.Decide(confidence, settings),
            active.Version);
    }

    private async Task<MlModelVersion?> GetActiveAsync()
    {
        if (!_activeLoaded)
        {
            _active = await models.GetActiveAsync();
            _activeLoaded = true;
        }

        return _active;
    }

    private async Task<IReadOnlyDictionary<Guid, Category>> GetCategoriesAsync(Guid userId)
    {
        if (!_categoryCache.TryGetValue(userId, out var cached))
        {
            cached = (await categories.GetAvailableAsync(userId)).ToDictionary(c => c.Id);
            _categoryCache[userId] = cached;
        }

        return cached;
    }
}
