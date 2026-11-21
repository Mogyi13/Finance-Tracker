using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Anomalies;

/// <summary>UC-25: a mentett anomáliák megjelenítése (nem számol újra).</summary>
public sealed class GetAnomaliesHandler(ICurrentUser currentUser, IAnomalyQueries queries, AnomalySettings settings)
{
    public async Task<Result<AnomaliesDto>> HandleAsync(bool includeReviewed = false)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        return Result<AnomaliesDto>.Ok(await AnomalyViews.BuildAsync(userId, queries, settings, includeReviewed));
    }
}

/// <summary>
/// UC-25: a felhasználó kiadásainak elemzése, az anomáliák felismerése és mentése. Az áttekintett állapot újraelemzéskor megmarad;
/// a már nem észlelt (a vizsgált tételekre vonatkozó) anomáliák törlődnek.
/// </summary>
public sealed class AnalyzeAnomaliesHandler(
    ICurrentUser currentUser,
    IAnomalyQueries queries,
    IAnomalyRepository repository,
    AnomalySettings settings,
    TimeProvider timeProvider)
{
    public async Task<Result<AnomalyAnalysisDto>> HandleAsync(double? threshold = null)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        if (threshold is { } t && (double.IsNaN(t) || t < AnomalySettings.MinAllowedThreshold || t > AnomalySettings.MaxAllowedThreshold))
        {
            var errors = new FieldErrors();
            errors.Add("threshold", $"A küszöb {AnomalySettings.MinAllowedThreshold:0.0} és {AnomalySettings.MaxAllowedThreshold:0.0} közötti z-pontszám lehet.");
            return errors.ToError();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        var lookbackStart = new DateOnly(today.Year, today.Month, 1).AddMonths(-settings.LookbackMonths);

        var expenses = await queries.GetExpensesSinceAsync(userId, lookbackStart);
        var detection = AnomalyDetector.Detect(expenses, today, settings, threshold);

        var existing = await repository.GetAllAsync(userId);
        var existingByKey = existing.ToDictionary(e => (e.TransactionId, e.AnomalyType));
        var detectedKeys = detection.Anomalies.Select(a => (a.TransactionId, a.Type)).ToHashSet();

        var toAdd = new List<FinancialAnomaly>();
        foreach (var anomaly in detection.Anomalies)
        {
            if (existingByKey.TryGetValue((anomaly.TransactionId, anomaly.Type), out var current))
            {
                current.Refresh(anomaly.Score, anomaly.Description, now);
            }
            else
            {
                toAdd.Add(FinancialAnomaly.Create(Guid.NewGuid(), userId, anomaly.TransactionId, anomaly.Type, anomaly.Score, anomaly.Description, now));
            }
        }

        var toRemove = existing
            .Where(e => detection.TestedTransactionIds.Contains(e.TransactionId) && !detectedKeys.Contains((e.TransactionId, e.AnomalyType)))
            .ToList();

        await repository.SaveAsync(toAdd, toRemove);

        var notice = detection.HadEnoughData
            ? null
            : $"Kevés az előzményadat az anomáliák megbízható felismeréséhez: kategóriánként legalább {settings.MinBaselineTransactions + 1} kiadás " +
              $"vagy {settings.MinBaselineMonths + 1} hónapnyi előzmény kell. Addig nem jelzünk, hogy ne legyen félrevezető az eredmény.";

        var view = await AnomalyViews.BuildAsync(userId, queries, settings, includeReviewed: false);
        return Result<AnomalyAnalysisDto>.Ok(new AnomalyAnalysisDto(view, detection.HadEnoughData, notice, toAdd.Count));
    }
}

/// <summary>UC-25: egy anomália áttekintettnek jelölése (a felhasználó látta és értékelte).</summary>
public sealed class ReviewAnomalyHandler(ICurrentUser currentUser, IAnomalyRepository repository)
{
    public async Task<Result<bool>> HandleAsync(Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        // A tulajdonos ellenőrzése: más felhasználó anomáliája ugyanúgy „nem található”.
        var anomaly = await repository.GetByIdAsync(userId, id);
        if (anomaly is null)
        {
            return Error.NotFound("Az anomália nem található.");
        }

        anomaly.MarkReviewed();
        await repository.SaveAsync(Array.Empty<FinancialAnomaly>(), Array.Empty<FinancialAnomaly>());
        return Result<bool>.Ok(true);
    }
}

internal static class AnomalyViews
{
    public static async Task<AnomaliesDto> BuildAsync(Guid userId, IAnomalyQueries queries, AnomalySettings settings, bool includeReviewed)
    {
        // Az áttekintett elemek nélkül is kell a számláló, ezért mindig a teljes listából számolunk.
        var all = await queries.GetAsync(userId, includeReviewed: true);
        var items = includeReviewed ? all : all.Where(a => !a.IsReviewed).ToList();

        return new AnomaliesDto(
            items,
            all.Count(a => !a.IsReviewed),
            all.Count == 0 ? null : all.Max(a => a.DetectedAt),
            settings.AmountZScoreThreshold,
            settings.CategoryZScoreThreshold);
    }
}
