namespace FinanceTracker.Application.Features.Budgets;

/// <summary>UC-27: a költségkeret-figyelmeztetés beállítása (konfiguráció: <c>Budgets</c>).</summary>
public sealed class BudgetSettings
{
    public const string SectionName = "Budgets";

    /// <summary>A keret ekkora hányadának elérésekor figyelmeztetünk (0,80 = 80%). Az érték (0, 1) közötti.</summary>
    public decimal WarningThreshold { get; set; } = 0.80m;
}
