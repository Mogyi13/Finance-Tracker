using System.Net.Mail;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// A rendszer felhasználója. A hitelesítő adatok (jelszó hash, zárolás stb.) nem a domain részei,
/// azokat az Infrastructure rétegbeli Identity kezeli; ez az entitás az üzleti szabályokat hordozza.
/// </summary>
public sealed class User
{
    public const int MaxEmailLength = 256;
    public const int MaxDisplayNameLength = 100;

    private User(Guid id, string email, string displayName, DateTime createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public string Email { get; }

    public string DisplayName { get; private set; }

    /// <summary>Regisztráció időpontja (UTC).</summary>
    public DateTime CreatedAt { get; }

    /// <summary>Új felhasználó létrehozása; az invariánsokat ellenőrzi.</summary>
    public static User Create(Guid id, string email, string displayName, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("A felhasználó azonosítója nem lehet üres.");
        }

        EnsureValidEmail(email);
        EnsureValidDisplayName(displayName);

        return new User(id, email, displayName, createdAtUtc);
    }

    /// <summary>Már tárolt felhasználó visszaállítása a perzisztenciából (nem ellenőriz újra).</summary>
    public static User Restore(Guid id, string email, string displayName, DateTime createdAtUtc)
        => new(id, email, displayName, createdAtUtc);

    public void ChangeDisplayName(string displayName)
    {
        EnsureValidDisplayName(displayName);
        DisplayName = displayName;
    }

    public static bool IsValidEmail(string? email)
        => !string.IsNullOrWhiteSpace(email)
           && email.Length <= MaxEmailLength
           && MailAddress.TryCreate(email, out var parsed)
           && parsed.Address == email;

    public static bool IsValidDisplayName(string? displayName)
        => !string.IsNullOrWhiteSpace(displayName)
           && displayName.Length <= MaxDisplayNameLength;

    private static void EnsureValidEmail(string email)
    {
        if (!IsValidEmail(email))
        {
            throw new DomainException("Az e-mail-cím formátuma érvénytelen.");
        }
    }

    private static void EnsureValidDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
        {
            throw new DomainException(
                $"A megjelenítendő név kötelező, és legfeljebb {MaxDisplayNameLength} karakter lehet.");
        }
    }
}
