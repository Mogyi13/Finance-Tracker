namespace FinanceTracker.Application.Features.Imports.PreviewImport;

/// <summary>UC-11 előkészítő lépés: a fájl szerkezetének és a javasolt oszlop-hozzárendelésnek lekérése.</summary>
public sealed record PreviewImportQuery(string FileName, Stream Content, ImportDelimiter Delimiter = ImportDelimiter.Auto);
