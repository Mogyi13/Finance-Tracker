using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Forecasting;

public sealed record MonthPointDto(DateOnly Month, decimal Amount);

public sealed record ForecastPointDto(DateOnly PeriodStart, DateOnly PeriodEnd, decimal Amount);

/// <summary>Egy módszer előrejelzései egy pénznemre.</summary>
public sealed record MethodForecastDto(ForecastMethod Method, IReadOnlyList<ForecastPointDto> Points);

/// <summary>
/// Egy pénznem előzményei és előrejelzései. Ha nincs elég előzmény, a <see cref="Methods"/> üres és a <see cref="Warning"/> ezt jelzi;
/// kevés előzménynél (<see cref="LowData"/>) van előrejelzés, de figyelmeztetéssel. A pénznemek nincsenek összevonva (BR-09).
/// </summary>
public sealed record ForecastCurrencyDto(
    string Currency,
    int HistoryMonths,
    IReadOnlyList<MonthPointDto> History,
    IReadOnlyList<MethodForecastDto> Methods,
    bool LowData,
    string? Warning);

/// <summary>Az előrejelzés mindig becslés, nem garantált érték (BR-10): a <see cref="Disclaimer"/> ezt kimondja.</summary>
public sealed record ForecastDto(
    ForecastType Type,
    DateTime? GeneratedAt,
    int MovingAverageWindow,
    string Disclaimer,
    IReadOnlyList<ForecastCurrencyDto> Currencies);

public sealed record MethodMetricsDto(ForecastMethod Method, decimal Mae, decimal Rmse, decimal? Mape);

public sealed record ForecastEvaluationCurrencyDto(
    string Currency,
    int TestedMonths,
    IReadOnlyList<MethodMetricsDto> Methods,
    ForecastMethod? BestMethod,
    string? Notice);

/// <summary>A módszerek összehasonlítása visszamenőleges (egy lépéses) teszttel a felhasználó saját előzményein.</summary>
public sealed record ForecastEvaluationDto(
    ForecastType Type,
    int MovingAverageWindow,
    string Explanation,
    IReadOnlyList<ForecastEvaluationCurrencyDto> Currencies);
