namespace FinanceTracker.Application.Features.Profile.UpdateProfile;

/// <summary>UC-04: profil módosítása (első körben csak a megjelenítendő név).</summary>
public sealed record UpdateProfileCommand(string DisplayName);
