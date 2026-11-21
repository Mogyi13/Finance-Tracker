using FinanceTracker.Application.Features.Budgets;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>A költségkeretek, anomáliák és előrejelzések egységes magyar megjelenítése (UC-25..UC-28).</summary>
public static class PlanningFormat
{
    public static string BudgetLevelText(BudgetLevel level) => level switch
    {
        BudgetLevel.Warning => "Közel a kerethez",
        BudgetLevel.Reached => "Keret kimerítve",
        BudgetLevel.Exceeded => "Túllépve",
        _ => "Rendben"
    };

    public static string BudgetLevelChip(BudgetLevel level) => level switch
    {
        BudgetLevel.Warning => "chip-warning",
        BudgetLevel.Reached or BudgetLevel.Exceeded => "chip-danger",
        _ => "chip-success"
    };

    public static string BudgetBarClass(BudgetLevel level) => level switch
    {
        BudgetLevel.Warning => "bar-warning",
        BudgetLevel.Reached or BudgetLevel.Exceeded => "bar-danger",
        _ => "bar-ok"
    };

    public static string PeriodTypeText(BudgetPeriodType type) => type switch
    {
        BudgetPeriodType.Weekly => "Heti",
        BudgetPeriodType.Custom => "Egyedi időszak",
        _ => "Havi"
    };

    public static string AnomalyTypeText(AnomalyType type) => type switch
    {
        AnomalyType.CategoryDeviation => "Szokatlan havi kategóriakiadás",
        _ => "Szokatlanul magas összeg"
    };

    public static string MethodText(ForecastMethod method) => method switch
    {
        ForecastMethod.LinearRegression => "Lineáris regresszió",
        _ => "Mozgóátlag"
    };

    public static string MonthLabel(DateOnly month) => month.ToString("yyyy. MM.", TransactionFormat.Hu);

    public static string Percent(decimal ratio) => ratio.ToString("P0", TransactionFormat.Hu);
}
