using Microsoft.ML.Data;

namespace FinanceTracker.Infrastructure.MachineLearning;

/// <summary>A modell bemenete. A Label a tanításnál a kategória azonosítója; predikciónál üres (a mentett csővezeték elvárja az oszlopot).</summary>
public sealed class ModelInput
{
    public string Text { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public sealed class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    /// <summary>Osztályonkénti valószínűségek (a maximum entrópia modell softmax kimenete), összegük 1.</summary>
    public float[] Score { get; set; } = Array.Empty<float>();
}
