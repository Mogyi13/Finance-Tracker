using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categories.UpdateCategory;

/// <summary>UC-10 kérés: saját kategória nevének és típusának módosítása.</summary>
public sealed record UpdateCategoryCommand(string Name, CategoryType Type);
