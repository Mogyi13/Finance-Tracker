using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Features.Categorization.PredictCategory;

/// <summary>Egy (még nem mentett) tranzakció típusa és leírása, amelyhez kategóriajavaslatot kérünk.</summary>
public sealed record PredictCategoryCommand(TransactionType Type, string? Description);
