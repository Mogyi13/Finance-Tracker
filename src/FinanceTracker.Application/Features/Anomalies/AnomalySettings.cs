namespace FinanceTracker.Application.Features.Anomalies;

/// <summary>
/// UC-25: az anomáliafelismerés beállításai (konfiguráció: <c>Analytics:Anomalies</c>). Az alapértékek a használt statisztikai
/// módszerek szokásos értékei; a küszöbök a konfigurációban (és az elemzés hívásakor, megengedett tartományon belül) módosíthatók.
/// </summary>
public sealed class AnomalySettings
{
    public const string SectionName = "Analytics:Anomalies";

    /// <summary>A kérésben megadható küszöb alsó és felső határa (z-pontszám).</summary>
    public const double MinAllowedThreshold = 1.5;

    public const double MaxAllowedThreshold = 6.0;

    /// <summary>Szokatlanul magas tranzakcióösszeg: a z-pontszám küszöbe (a hasonló tranzakciók átlagától mért szórásszám).</summary>
    public double AmountZScoreThreshold { get; set; } = 3.0;

    /// <summary>Szokatlan havi kategóriakiadás: a z-pontszám küszöbe.</summary>
    public double CategoryZScoreThreshold { get; set; } = 2.5;

    /// <summary>Legalább ennyi, a vizsgált tételen kívüli tranzakció kell egy (kategória, pénznem) csoportban a megbízható alapvonalhoz.</summary>
    public int MinBaselineTransactions { get; set; } = 8;

    /// <summary>Legalább ennyi, a vizsgált hónapon kívüli hónap kell a kategória havi alapvonalához.</summary>
    public int MinBaselineMonths { get; set; } = 4;

    /// <summary>Az eltérésnek az átlaghoz képest legalább ekkora relatív növekedést is el kell érnie (0,25 = +25%), hogy a nagyon stabil kiadásoknál ne jelezzen apró eltérést.</summary>
    public double MinRelativeIncrease { get; set; } = 0.25;

    /// <summary>Az utolsó ennyi nap tranzakcióit vizsgáljuk (a régebbiek csak az alapvonalat adják).</summary>
    public int AnalysisWindowDays { get; set; } = 90;

    /// <summary>Az utolsó ennyi naptári hónapot (az aktuálissal együtt) vizsgáljuk a kategóriaeltérésnél.</summary>
    public int AnalysisWindowMonths { get; set; } = 3;

    /// <summary>Az alapvonalhoz felhasznált előzmények hossza hónapban.</summary>
    public int LookbackMonths { get; set; } = 12;
}
