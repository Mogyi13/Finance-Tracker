using Microsoft.AspNetCore.Authorization;

namespace FinanceTracker.API.Infrastructure;

public static class AuthorizationPolicies
{
    /// <summary>UC-20: az ML-modell tanítását, kiértékelését és aktiválását végző üzemeltető/fejlesztő.</summary>
    public const string ModelAdministrator = "ModelAdministrator";

    /// <summary>A konfigurációs kulcs, amely az üzemeltetők e-mail-címeit tartalmazza (tömb).</summary>
    public const string ModelAdministratorEmailsKey = "Categorization:Ml:AdminEmails";

    /// <summary>
    /// A bejelentkezett felhasználó akkor üzemeltető, ha az e-mail-címe szerepel a (szerveroldali) konfigurációban.
    /// Ez a jogosultság kizárólag a modellkezelő végpontokra szól: más felhasználók pénzügyi adatait nem tárja fel.
    /// </summary>
    public static void AddModelAdministratorPolicy(this AuthorizationOptions options, IEnumerable<string> adminEmails)
    {
        var emails = new HashSet<string>(
            adminEmails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()),
            StringComparer.OrdinalIgnoreCase);

        options.AddPolicy(ModelAdministrator, policy => policy
            .RequireAuthenticatedUser()
            .RequireAssertion(context =>
            {
                var email = context.User.FindFirst("email")?.Value;
                return email is not null && emails.Contains(email);
            }));
    }
}
