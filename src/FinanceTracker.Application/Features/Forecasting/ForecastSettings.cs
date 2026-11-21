namespace FinanceTracker.Application.Features.Forecasting;

/// <summary>UC-28: az előrejelzés beállításai (konfiguráció: <c>Forecasting</c>). A mozgóátlag ablaka állítható (SF-038).</summary>
public sealed class ForecastSettings
{
    public const string SectionName = "Forecasting";

    /// <summary>Mozgóátlag ablaka hónapban; az érvényes tartomány 2..12 (a kényszerítést a <see cref="EffectiveWindow"/> végzi).</summary>
    public int MovingAverageWindow { get; set; } = 3;

    /// <summary>Ennyi lezárt hónapnál kevesebb előzményből nem készül előrejelzés (BR-13). Legalább az ablak mérete.</summary>
    public int MinHistoryMonths { get; set; } = 4;

    /// <summary>Ennyi hónapnál rövidebb előzménynél az előrejelzés figyelmeztetést kap (SF-041).</summary>
    public int LowDataMonths { get; set; } = 6;

    /// <summary>A legfeljebb ennyi legutóbbi lezárt hónapot használjuk.</summary>
    public int MaxHistoryMonths { get; set; } = 24;

    /// <summary>Hány hónapra előre jelzünk (1..6).</summary>
    public int HorizonMonths { get; set; } = 3;

    public int EffectiveWindow => Math.Clamp(MovingAverageWindow, 2, 12);

    public int EffectiveMinHistory => Math.Max(Math.Max(MinHistoryMonths, 3), EffectiveWindow);

    public int EffectiveHorizon => Math.Clamp(HorizonMonths, 1, 6);

    public int EffectiveMaxHistory => Math.Clamp(MaxHistoryMonths, EffectiveMinHistory, 60);
}
