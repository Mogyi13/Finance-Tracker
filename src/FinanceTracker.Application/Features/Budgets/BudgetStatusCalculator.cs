namespace FinanceTracker.Application.Features.Budgets;

/// <summary>UC-27: a felhasználtság és a figyelmeztetési szint számítása (tiszta logika).</summary>
public static class BudgetStatusCalculator
{
    public static (decimal Remaining, decimal UsedRatio, BudgetLevel Level) Calculate(decimal amount, decimal spent, decimal warningThreshold)
    {
        var remaining = amount - spent;
        var ratio = amount > 0 ? Math.Round(spent / amount, 4) : 0m;

        var level = spent > amount ? BudgetLevel.Exceeded
            : spent == amount ? BudgetLevel.Reached
            : spent >= amount * warningThreshold ? BudgetLevel.Warning
            : BudgetLevel.Ok;

        return (remaining, ratio, level);
    }
}
