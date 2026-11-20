using System.Text.Json.Serialization;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>
/// Az ML-alapú kategorizálás beállításai (konfigurációs szakasz: <see cref="SectionName"/>).
/// A küszöbök (UC-19) és az aktiválási feltételek (UC-20) kezdőértékek: a végleges értékeket a modellkiértékelés
/// jelentése (küszöbelemzés: lefedettség és pontosság küszöbönként) alapján kell finomhangolni.
/// </summary>
public sealed class MlSettings
{
    public const string SectionName = "Categorization:Ml";

    // --- UC-19: megbízhatósági küszöbök

    /// <summary>Ennél nagyobb vagy egyenlő megbízhatóságnál a kategória automatikusan hozzárendelődik (magas megbízhatóság).</summary>
    public double AutoAssignThreshold { get; set; } = 0.80;

    /// <summary>Az "alacsony" megbízhatóság felső határa: ettől felfelé (az automatikus küszöb alatt) közepes a megbízhatóság.</summary>
    public double MediumThreshold { get; set; } = 0.40;

    /// <summary>Ennél kisebb megbízhatóságú predikciót a rendszer nem is javasol.</summary>
    public double MinSuggestionConfidence { get; set; } = 0.20;

    /// <summary>A javaslatok oldalán egyszerre vizsgált kategorizálatlan tranzakciók legnagyobb száma.</summary>
    public int MaxSuggestionScan { get; set; } = 50;

    // --- UC-20: tanítás

    /// <summary>A tisztítás után szükséges tanítópéldák legkisebb száma (tanító + teszt együtt).</summary>
    public int MinTrainingExamples { get; set; } = 50;

    /// <summary>Az ennél kevesebb példával rendelkező kategóriák kimaradnak a tanításból.</summary>
    public int MinExamplesPerClass { get; set; } = 3;

    /// <summary>A tesztadatok legkisebb száma; ennél kevesebb teszttel a mutatók nem értelmezhetők.</summary>
    public int MinTestExamples { get; set; } = 20;

    /// <summary>A teszthalmaz aránya (0 és 1 között).</summary>
    public double TestFraction { get; set; } = 0.2;

    /// <summary>A felosztás és a tanítás magértéke (a futások így reprodukálhatók).</summary>
    public int Seed { get; set; } = 42;

    // --- UC-20: aktiválási feltételek

    public double MinAccuracy { get; set; } = 0.70;

    public double MinMacroF1 { get; set; } = 0.60;

    public double MaxAveragePredictionMs { get; set; } = 50;

    /// <summary>Az új modell pontossága legfeljebb ennyivel lehet kisebb az éppen aktív modellénél (0: nem lehet rosszabb).</summary>
    public double MaxRegressionVsActive { get; set; } = 0.0;

    // --- Modelltár

    /// <summary>A modellfájlok könyvtára (relatív útvonal az alkalmazás könyvtárához képest, vagy abszolút). Csak a szerver állítja.</summary>
    [JsonIgnore]
    public string ModelDirectory { get; set; } = "models";
}
