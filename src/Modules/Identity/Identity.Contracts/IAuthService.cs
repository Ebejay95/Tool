using Identity.Application.DTOs;
using SharedKernel;

namespace Identity.Application;

/// <summary>
/// Port (Hexagonal Architecture): Framework-agnostisches Interface für
/// Authentifizierungsoperationen. Wird von Blazor-Komponenten über @inject genutzt.
/// Die Implementierung (<c>AuthService</c>) lebt in Api, da sie
/// Blazor-spezifische Abhängigkeiten (JSRuntime, CircuitTokenHolder) hat.
/// </summary>
public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto);

    Task<Result<UserDto>> RegisterAsync(RegisterUserDto dto);

    Task<bool> IsAuthenticatedAsync();

    Task<string?> GetTokenAsync();

    Task<UserInfo?> GetCurrentUserAsync();

    Task LogoutAsync();

    Task<Result> RequestPasswordResetAsync(string email);

    Task<Result> ResetPasswordAsync(string token, string newPassword);

    Task<Result> ChangePasswordAsync(ChangePasswordDto dto);

    /// <summary>Initialisiert den Auth-Zustand aus dem Browser-LocalStorage (nach Page-Load).</summary>
    Task InitializeAsync();

    // ── E-Mail-Verifizierung ─────────────────────────────────────────────────

    /// <summary>Verifiziert die E-Mail-Adresse mit einem Token aus der Bestätigungs-E-Mail.</summary>
    Task<Result> VerifyEmailAsync(VerifyEmailDto dto);

    /// <summary>
    /// Fordert eine neue Bestätigungs-E-Mail an (Rate-Limit: 1×/Stunde).
    /// Gibt immer Success zurück, um keine Benutzerexistenz zu offenbaren.
    /// </summary>
    Task<Result> ResendVerificationEmailAsync(ResendVerificationEmailDto dto);

    // ── Zwei-Faktor-Authentifizierung ────────────────────────────────────────

    /// <summary>
    /// Initiiert das 2FA-Setup. Erfordert einen PreAuthToken mit Stage "2fa_setup".
    /// Gibt Secret-Key und QR-Code zurück.
    /// </summary>
    Task<Result<TwoFactorSetupDto>> InitiateTwoFactorSetupAsync(string preAuthToken);

    /// <summary>
    /// Bestätigt die 2FA-Einrichtung mit dem ersten gültigen Code aus der App.
    /// Bei Erfolg wird ein vollständiges JWT-Token ausgestellt.
    /// </summary>
    Task<Result<LoginResponseDto>> ConfirmTwoFactorSetupAsync(ConfirmTwoFactorDto dto);

    /// <summary>
    /// Validiert einen TOTP-Code während des Logins.
    /// Bei Erfolg wird ein vollständiges JWT-Token ausgestellt.
    /// </summary>
    Task<Result<LoginResponseDto>> ValidateTwoFactorAsync(ValidateTwoFactorDto dto);
}
