namespace FinanceTracker.Application.Features.Categorization.CorrectCategory;

/// <summary>UC-17: egy tranzakció kategóriájának kézi javítása (vagy egy javaslat megerősítése).</summary>
/// <param name="TransactionId">A javítandó tranzakció.</param>
/// <param name="CategoryId">A felhasználó által választott kategória.</param>
/// <param name="PredictedCategoryId">Opcionális: a felhasználónak megjelenített javaslat kategóriája. Ha nincs megadva, a tranzakció jelenlegi automatikus kategóriája számít javaslatnak.</param>
public sealed record CorrectCategoryCommand(Guid TransactionId, Guid CategoryId, Guid? PredictedCategoryId = null);
