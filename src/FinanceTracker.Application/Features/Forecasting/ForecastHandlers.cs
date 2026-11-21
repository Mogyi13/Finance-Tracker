using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Forecasting;

internal static class ForecastText
{
    public const string Disclaimer =
        "Az előrejelzés a korábbi adatokból számolt becslés, nem garantált érték. Váratlan események és szokásváltozás eltérést okozhatnak.";

    public static bool IsValid(ForecastType type) => Enum.IsDefined(type);

    public static Error InvalidType()
    {
        var errors = new FieldErrors();
        errors.Add("type", "Az előrejelzés típusa érvénytelen.");
        return errors.ToError();
    }
}

/// <summary>UC-28: előrejelzés számítása és mentése (a korábbi mentett eredmények lecserélődnek).</summary>
public sealed class GenerateForecastHandler(
    ICurrentUser currentUser,
    IForecastQueries queries,
    IForecastRepository repository,
    ForecastSettings settings,
    TimeProvider timeProvider)
{
    public async Task<Result<ForecastDto>> HandleAsync(ForecastType type)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        if (!ForecastText.IsValid(type))
        {
            return ForecastText.InvalidType();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        var history = await ForecastLoader.LoadHistoryAsync(queries, userId, type, today, settings);

        var results = new List<ForecastResult>();
        var currencies = new List<ForecastCurrencyDto>();
        var horizon = settings.EffectiveHorizon;
        var firstForecastMonth = ForecastSeries.LastCompleteMonth(today).AddMonths(1);

        foreach (var (currency, points) in history)
        {
            var values = points.Select(p => p.Amount).ToList();
            if (values.Count < settings.EffectiveMinHistory)
            {
                currencies.Add(ForecastCurrencyBuilder.Insufficient(currency, points, settings));
                continue;
            }

            var nonNegative = type == ForecastType.MonthlyExpenses;
            var ma = ForecastCalculator.MovingAverage(values, settings.EffectiveWindow, horizon);
            var lr = ForecastCalculator.LinearRegression(values, horizon, nonNegative);

            // Módszerenként külön rekord (spec.): a mozgóátlag és a lineáris regresszió külön tárolódik.
            for (var h = 0; h < horizon; h++)
            {
                var start = firstForecastMonth.AddMonths(h);
                var end = start.AddMonths(1).AddDays(-1);
                results.Add(ForecastResult.Create(Guid.NewGuid(), userId, type, ForecastMethod.MovingAverage, currency, start, end, ma[h], now));
                results.Add(ForecastResult.Create(Guid.NewGuid(), userId, type, ForecastMethod.LinearRegression, currency, start, end, lr[h], now));
            }

            currencies.Add(ForecastCurrencyBuilder.Build(currency, points, ma, lr, firstForecastMonth, settings));
        }

        await repository.ReplaceAsync(userId, type, results);

        return Result<ForecastDto>.Ok(new ForecastDto(type, now, settings.EffectiveWindow, ForecastText.Disclaimer, currencies));
    }
}

/// <summary>UC-28: a mentett előrejelzés lekérése az előzményekkel; ha még nincs mentett eredmény, a GeneratedAt null és nincs becslés.</summary>
public sealed class GetForecastHandler(ICurrentUser currentUser, IForecastQueries queries, ForecastSettings settings, TimeProvider timeProvider)
{
    public async Task<Result<ForecastDto>> HandleAsync(ForecastType type)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        if (!ForecastText.IsValid(type))
        {
            return ForecastText.InvalidType();
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var history = await ForecastLoader.LoadHistoryAsync(queries, userId, type, today, settings);
        var saved = await queries.GetSavedAsync(userId, type);

        var currencies = new List<ForecastCurrencyDto>();
        foreach (var (currency, points) in history)
        {
            var rows = saved.Where(s => s.Currency == currency).ToList();
            if (points.Count < settings.EffectiveMinHistory)
            {
                currencies.Add(ForecastCurrencyBuilder.Insufficient(currency, points, settings));
                continue;
            }

            var methods = rows
                .GroupBy(r => r.Method)
                .OrderBy(g => g.Key)
                .Select(g => new MethodForecastDto(
                    g.Key,
                    g.OrderBy(r => r.PeriodStart).Select(r => new ForecastPointDto(r.PeriodStart, r.PeriodEnd, r.PredictedAmount)).ToList()))
                .ToList();

            currencies.Add(new ForecastCurrencyDto(currency, points.Count, points, methods,
                points.Count < settings.LowDataMonths, ForecastCurrencyBuilder.LowDataWarning(points.Count, settings)));
        }

        var generatedAt = saved.Count == 0 ? (DateTime?)null : saved.Max(s => s.CreatedAt);
        return Result<ForecastDto>.Ok(new ForecastDto(type, generatedAt, settings.EffectiveWindow, ForecastText.Disclaimer, currencies));
    }
}

/// <summary>UC-28: a két módszer pontosságának összehasonlítása a felhasználó saját előzményein.</summary>
public sealed class EvaluateForecastHandler(ICurrentUser currentUser, IForecastQueries queries, ForecastSettings settings, TimeProvider timeProvider)
{
    public async Task<Result<ForecastEvaluationDto>> HandleAsync(ForecastType type)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        if (!ForecastText.IsValid(type))
        {
            return ForecastText.InvalidType();
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var history = await ForecastLoader.LoadHistoryAsync(queries, userId, type, today, settings);

        var currencies = history
            .Select(h => ForecastEvaluator.Evaluate(h.Key, h.Value.Select(p => p.Amount).ToList(), type, settings))
            .ToList();

        return Result<ForecastEvaluationDto>.Ok(new ForecastEvaluationDto(
            type,
            settings.EffectiveWindow,
            "Minden hónapot a megelőző hónapokból becsültünk, és összevetettük a tényleges értékkel. Kisebb hiba = pontosabb módszer. A múltbeli pontosság nem garantálja a jövőbelit.",
            currencies));
    }
}

internal static class ForecastLoader
{
    public static async Task<IReadOnlyDictionary<string, IReadOnlyList<MonthPointDto>>> LoadHistoryAsync(
        IForecastQueries queries, Guid userId, ForecastType type, DateOnly today, ForecastSettings settings)
    {
        var from = ForecastSeries.FirstMonthNeeded(today, settings.EffectiveMaxHistory);
        var to = ForecastSeries.LastCompleteMonth(today).AddMonths(1).AddDays(-1);
        var totals = await queries.GetMonthlyTotalsAsync(userId, from, to);
        return ForecastSeries.Build(totals, type, today, settings.EffectiveMaxHistory);
    }
}

internal static class ForecastCurrencyBuilder
{
    public static string? LowDataWarning(int months, ForecastSettings settings) => months < settings.LowDataMonths
        ? $"Kevés előzményből ({months} hónap) készült, ezért a becslés bizonytalanabb. Legalább {settings.LowDataMonths} hónap javasolt."
        : null;

    public static ForecastCurrencyDto Insufficient(string currency, IReadOnlyList<MonthPointDto> points, ForecastSettings settings) =>
        new(currency, points.Count, points, Array.Empty<MethodForecastDto>(), true,
            $"Nincs elég előzményadat az előrejelzéshez: legalább {settings.EffectiveMinHistory} lezárt hónap kell, jelenleg {points.Count} van.");

    public static ForecastCurrencyDto Build(
        string currency, IReadOnlyList<MonthPointDto> points, IReadOnlyList<decimal> ma, IReadOnlyList<decimal> lr,
        DateOnly firstForecastMonth, ForecastSettings settings)
    {
        ForecastPointDto Point(int h, decimal amount)
        {
            var start = firstForecastMonth.AddMonths(h);
            return new ForecastPointDto(start, start.AddMonths(1).AddDays(-1), amount);
        }

        var methods = new List<MethodForecastDto>
        {
            new(ForecastMethod.MovingAverage, ma.Select((a, h) => Point(h, a)).ToList()),
            new(ForecastMethod.LinearRegression, lr.Select((a, h) => Point(h, a)).ToList())
        };

        return new ForecastCurrencyDto(currency, points.Count, points, methods,
            points.Count < settings.LowDataMonths, LowDataWarning(points.Count, settings));
    }
}
