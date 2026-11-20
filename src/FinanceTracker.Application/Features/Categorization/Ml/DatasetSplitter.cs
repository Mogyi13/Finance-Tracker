using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Application.Features.Categorization.Ml;

/// <summary>
/// A tanító- és a teszthalmaz szétválasztása. A besorolás a tranzakció azonosítójának és a magértéknek a hash-éből
/// adódik, ezért determinisztikus és stabil: ugyanaz a tranzakció minden futásban ugyanabba a halmazba kerül
/// (új adatok érkezésekor sem vándorol át), így a teszthalmaz nem szivárog a tanításba (data leakage nélkül).
/// </summary>
public static class DatasetSplitter
{
    public static (List<T> Train, List<T> Test) Split<T>(IEnumerable<T> items, Func<T, Guid> idOf, double testFraction, int seed)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(idOf);

        var train = new List<T>();
        var test = new List<T>();
        foreach (var item in items)
        {
            (IsTest(idOf(item), testFraction, seed) ? test : train).Add(item);
        }

        return (train, test);
    }

    public static bool IsTest(Guid id, double testFraction, int seed)
        => Bucket(id, seed) < testFraction;

    /// <summary>A 0 (zárt) és 1 (nyílt) közé eső, egyenletes eloszlású érték az azonosítóból.</summary>
    public static double Bucket(Guid id, int seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{seed}:{id:N}"));
        var value = BinaryPrimitives.ReadUInt64LittleEndian(hash);
        return value / ((double)ulong.MaxValue + 1.0);
    }
}
