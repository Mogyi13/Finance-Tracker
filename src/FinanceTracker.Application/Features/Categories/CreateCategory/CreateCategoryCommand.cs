using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categories.CreateCategory;

/// <summary>UC-10 kérés: új saját kategória.</summary>
public sealed record CreateCategoryCommand(string Name, CategoryType Type);
