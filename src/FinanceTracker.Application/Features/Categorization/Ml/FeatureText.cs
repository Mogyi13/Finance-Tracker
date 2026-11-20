using System.Text;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>
/// UC-18: a tranzakció leírásának normalizálása a modell bemeneti szövegévé. A tanításnál és a predikciónál
/// ugyanezt a függvényt kell használni, különben a modell mást lát, mint amin tanult.
/// Lépések: kisbetűsítés, magyar ékezetek elhagyása, a számok és írásjelek eldobása (szóelválasztóvá válnak),
/// a túl rövid szavak kihagyása, végül a tranzakció típusát jelző kulcsszó elé írása.
/// </summary>
public static class FeatureText
{
    public const int MinTokenLength = 2;
    public const int MaxTokens = 40;

    /// <summary>A modell bemeneti szövege, vagy null, ha a leírásból nem marad használható szó.</summary>
    public static string? Build(TransactionType type, string? description)
    {
        var tokens = Tokenize(description);
        if (tokens.Count == 0)
        {
            return null;
        }

        return $"{TypeToken(type)} {string.Join(' ', tokens)}";
    }

    public static IReadOnlyList<string> Tokenize(string? description)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(description))
        {
            return tokens;
        }

        var current = new StringBuilder();

        void Flush()
        {
            if (current.Length >= MinTokenLength && tokens.Count < MaxTokens)
            {
                tokens.Add(current.ToString());
            }

            current.Clear();
        }

        foreach (var raw in description)
        {
            var c = Fold(char.ToLowerInvariant(raw));
            if (char.IsLetter(c))
            {
                current.Append(c);
            }
            else
            {
                Flush();
            }
        }

        Flush();
        return tokens;
    }

    private static string TypeToken(TransactionType type) => type == TransactionType.Income ? "tipusbevetel" : "tipuskiadas";

    /// <summary>A magyar ékezetes betűk egyszerű megfelelője (a gyakori ékezet nélküli írásmóddal való egyezésért).</summary>
    private static char Fold(char c) => c switch
    {
        'á' => 'a',
        'é' => 'e',
        'í' => 'i',
        'ó' or 'ö' or 'ő' => 'o',
        'ú' or 'ü' or 'ű' => 'u',
        _ => c
    };
}
