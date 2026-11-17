using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Common.Abstractions;

/// <summary>A hitelesítő adatokkal kapcsolatos műveletek (regisztráció, jelszóellenőrzés, zárolás).</summary>
public interface IIdentityService
{
    /// <summary>Elmenti az új felhasználót a jelszavával. Validációs hibát ad, ha az e-mail foglalt vagy a jelszó gyenge.</summary>
    Task<Result<User>> RegisterAsync(User user, string password);

    /// <summary>Ellenőrzi a hitelesítő adatokat, kezeli a sikertelen próbálkozásokat és a zárolást.</summary>
    Task<Result<User>> VerifyCredentialsAsync(string email, string password);
}
