namespace FinanceTracker.Web.Services;

/// <summary>
/// A bejelentkezett felhasználó megjelenítendő neve a felületen (fejléc, avatar).
/// A JWT-ben csak az e-mail-cím szerepel, ezért a profil betöltésekor az oldalak ide teszik be a nevet.
/// </summary>
public sealed class UserDisplayState
{
    public string? DisplayName { get; private set; }

    public event Action? Changed;

    public void Set(string? displayName)
    {
        if (string.Equals(DisplayName, displayName, StringComparison.Ordinal))
        {
            return;
        }

        DisplayName = displayName;
        Changed?.Invoke();
    }
}
