using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Infrastructure.MachineLearning;

/// <summary>
/// A modellfájlok szerver által felügyelt tára. A fájlnevet mindig a szerver generálja (verzió + véletlen azonosító),
/// és minden feloldott útvonal a modellkönyvtáron belül marad, így felhasználói bemenet sosem jut fájlrendszeri útvonalba.
/// </summary>
public sealed class MlModelStore(MlSettings settings)
{
    public string Directory => Path.GetFullPath(settings.ModelDirectory, AppContext.BaseDirectory);

    public string NewFileName(string version)
    {
        var safeVersion = new string(version.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
        return $"model-{safeVersion}-{Guid.NewGuid():N}.zip";
    }

    public bool Exists(string fileName) => TryResolve(fileName, out var path) && File.Exists(path);

    /// <summary>A modellfájl teljes elérési útja; érvénytelen név vagy a könyvtárból kilépő útvonal esetén kivétel.</summary>
    public string Resolve(string fileName)
        => TryResolve(fileName, out var path)
            ? path
            : throw new MlModelException("A modellfájl neve érvénytelen.");

    public void Delete(string fileName)
    {
        if (TryResolve(fileName, out var path) && File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void EnsureDirectory() => System.IO.Directory.CreateDirectory(Directory);

    private bool TryResolve(string fileName, out string path)
    {
        path = string.Empty;
        if (!MlModelVersion.IsSafeFileName(fileName))
        {
            return false;
        }

        var root = Directory;
        var full = Path.GetFullPath(Path.Combine(root, fileName));
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            return false;
        }

        path = full;
        return true;
    }
}

/// <summary>Az Application által látott modelltár-port a <see cref="MlModelStore"/> fölött.</summary>
public sealed class MlModelStorePort(MlModelStore store) : IMlModelStore
{
    public string NewFileName(string version) => store.NewFileName(version);

    public bool Exists(string fileName) => store.Exists(fileName);

    public void Delete(string fileName) => store.Delete(fileName);
}
