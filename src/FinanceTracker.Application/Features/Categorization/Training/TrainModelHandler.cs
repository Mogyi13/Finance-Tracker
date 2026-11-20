using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categorization.Training;

/// <summary>
/// UC-20: modell tanítása és kiértékelése. A tanítóhalmazt a kézzel kategorizált tranzakciókból építi, megtisztítja és
/// normalizálja, determinisztikusan szétválasztja tanító- és teszthalmazra, betanítja a modellt, majd KIZÁRÓLAG a teszthalmazon
/// értékeli (pontosság, makro precision/recall/F1, átlagos predikciós idő), és összeveti a szabályalapú alapmegoldással,
/// a "legtöbbször előforduló kategória" tippelésével, valamint az éppen aktív modellel. Az eredmény új, INAKTÍV
/// modellverzió: az aktiválás külön, tudatos lépés (<see cref="ActivateModelHandler"/>), és csak a feltételeknek megfelelő
/// verzió aktiválható. A hívás jogosultságát az API réteg ellenőrzi; a válasz személyes pénzügyi adatot nem tartalmaz.
/// </summary>
public sealed class TrainModelHandler(
    ITrainingDataSource data,
    IMlModelTrainer trainer,
    IMlModelRuntime runtime,
    IMlModelStore store,
    IMlModelRepository models,
    ICategorizationRuleRepository rules,
    ICategoryRepository categories,
    MlSettings settings,
    TimeProvider timeProvider)
{
    // Egyszerre csak egy tanítás futhat (a tanítás erőforrás-igényes, és az eredmények összemosódnának).
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task<Result<ModelVersionDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        if (!await Gate.WaitAsync(0, cancellationToken))
        {
            return Error.Conflict("Már fut egy modelltanítás. Várd meg a befejezését.");
        }

        try
        {
            return await TrainAsync(cancellationToken);
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<Result<ModelVersionDto>> TrainAsync(CancellationToken cancellationToken)
    {
        var raw = await data.GetSamplesAsync(cancellationToken);

        // Tisztítás: használhatatlan szövegű minták eldobása, majd a túl ritka kategóriák kihagyása.
        var cleaned = raw
            .Select(s => (Sample: s, Features: FeatureText.Build(s.Type, s.Description)))
            .Where(x => x.Features is not null)
            .Select(x => new Prepared(x.Sample, x.Features!))
            .ToList();

        var perClass = cleaned.GroupBy(p => p.Sample.CategoryId).ToDictionary(g => g.Key, g => g.Count());
        var labelled = cleaned.Where(p => perClass[p.Sample.CategoryId] >= settings.MinExamplesPerClass).ToList();
        var classCount = labelled.Select(p => p.Sample.CategoryId).Distinct().Count();

        if (labelled.Count < settings.MinTrainingExamples || classCount < 2)
        {
            return Error.Conflict(
                $"Nincs elég tanítóadat: {labelled.Count} használható példa van {classCount} kategóriából, " +
                $"legalább {settings.MinTrainingExamples} példa és 2 kategória kell (kategóriánként legalább {settings.MinExamplesPerClass} példával).");
        }

        var (train, test) = DatasetSplitter.Split(labelled, p => p.Sample.TransactionId, settings.TestFraction, settings.Seed);
        if (test.Count < settings.MinTestExamples || train.Count == 0)
        {
            return Error.Conflict(
                $"Kevés a tesztadat: {test.Count} teszt- és {train.Count} tanítópélda adódott, " +
                $"legalább {settings.MinTestExamples} tesztpélda kell. Gyűjts több kézzel kategorizált tranzakciót.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var version = "v" + now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
        var fileName = store.NewFileName(version);

        var trainExamples = train.Select(p => new MlTrainingExample(p.Features, p.Sample.CategoryId)).ToList();
        var testExamples = test.Select(p => new EvaluationExample(p.Features, p.Sample.CategoryId)).ToList();

        MlModelVersion entity;
        var saved = false;
        try
        {
            try
            {
                trainer.Train(trainExamples, fileName);
            }
            catch (MlModelException ex)
            {
                return Error.Failure($"A modell tanítása sikertelen: {ex.Message}");
            }

            EvaluationRun run;
            try
            {
                run = ModelEvaluator.Evaluate(testExamples, f => runtime.Predict(fileName, f));
            }
            catch (MlModelException ex)
            {
                return Error.Failure($"A betanított modell kiértékelése sikertelen: {ex.Message}");
            }

            var report = ClassificationMetrics.Compute(run.Results.Select(r => (r.Actual, r.Predicted)).ToList());
            var thresholds = ClassificationMetrics.ThresholdAnalysis(run.Results);
            var majority = ClassificationMetrics.MajorityClassAccuracy(
                train.Select(p => p.Sample.CategoryId), test.Select(p => p.Sample.CategoryId).ToList());
            var overlap = ModelEvaluator.DescriptionOverlap(train.Select(p => p.Features), test.Select(p => p.Features).ToList());
            var (baselineAccuracy, baselineCoverage) = await EvaluateRuleBaselineAsync(test.Select(p => p.Sample).ToList());

            var (activeVersion, activeAccuracy) = await EvaluateActiveAsync(testExamples);

            var inputs = new ActivationInputs(
                test.Count, report.Accuracy, report.MacroF1, run.AveragePredictionMs, majority, baselineAccuracy, activeAccuracy);
            var criteria = ActivationCriteria.Evaluate(inputs, settings);
            var meets = ActivationCriteria.AllPassed(criteria);

            var evaluation = new ModelEvaluationReport(
                version, now, labelled.Count, classCount, train.Count, test.Count, settings.TestFraction, settings.Seed,
                report, run.AveragePredictionMs, baselineAccuracy, baselineCoverage, majority, activeVersion, activeAccuracy,
                overlap, thresholds, criteria, meets, settings);

            entity = MlModelVersion.Create(
                Guid.NewGuid(), version, fileName,
                ToMetric(report.Accuracy), ToMetric(report.MacroPrecision), ToMetric(report.MacroRecall), ToMetric(report.MacroF1),
                train.Count, test.Count, (decimal)Math.Round(run.AveragePredictionMs, 4), ToMetric(baselineAccuracy),
                meets, evaluation.ToJson(), now);

            await models.AddAsync(entity);
            saved = true;
        }
        finally
        {
            // Ha a verzió nem került az adatbázisba, a félig kész modellfájl nem maradhat árván a tárban.
            if (!saved)
            {
                store.Delete(fileName);
            }
        }

        return Result<ModelVersionDto>.Ok(ModelVersionDto.From(entity));
    }

    /// <summary>
    /// A szabályalapú alapmegoldás: a teszttranzakciók tulajdonosainak aktív szabályai mennyire találnák el a helyes kategóriát.
    /// A nem illeszkedő tétel hibának számít (a pontosság a teljes teszthalmazra vonatkozik), a lefedettség az illeszkedők aránya.
    /// </summary>
    private async Task<(double Accuracy, double Coverage)> EvaluateRuleBaselineAsync(IReadOnlyList<TrainingSample> test)
    {
        var correct = 0;
        var matched = 0;

        foreach (var owner in test.GroupBy(s => s.OwnerId))
        {
            var ownerRules = (await rules.GetAllAsync(owner.Key)).Where(r => r.IsActive).ToList();
            if (ownerRules.Count == 0)
            {
                continue;
            }

            var available = (await categories.GetAvailableAsync(owner.Key)).ToDictionary(c => c.Id);
            foreach (var sample in owner)
            {
                var match = RuleEvaluator.FindMatch(ownerRules, available, sample.Type, sample.Description);
                if (match is null)
                {
                    continue;
                }

                matched++;
                if (match.CategoryId == sample.CategoryId)
                {
                    correct++;
                }
            }
        }

        return test.Count == 0 ? (0, 0) : ((double)correct / test.Count, (double)matched / test.Count);
    }

    /// <summary>Az éppen aktív modell pontossága ugyanezen a teszthalmazon (ha van aktív modell, és a fájlja használható).</summary>
    private async Task<(string? Version, double? Accuracy)> EvaluateActiveAsync(IReadOnlyList<EvaluationExample> test)
    {
        var active = await models.GetActiveAsync();
        if (active is null || !store.Exists(active.ModelPath))
        {
            return (null, null);
        }

        try
        {
            var run = ModelEvaluator.Evaluate(test, f => runtime.Predict(active.ModelPath, f));
            var accuracy = ClassificationMetrics.Compute(run.Results.Select(r => (r.Actual, r.Predicted)).ToList()).Accuracy;
            return (active.Version, accuracy);
        }
        catch (MlModelException)
        {
            return (active.Version, null);
        }
    }

    private static decimal ToMetric(double value) => Math.Clamp((decimal)Math.Round(value, 6), 0m, 1m);

    private sealed record Prepared(TrainingSample Sample, string Features);
}
