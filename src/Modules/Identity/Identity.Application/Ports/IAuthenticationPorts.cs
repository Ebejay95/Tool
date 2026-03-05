using Identity.Domain.Users;
using SharedKernel;

namespace Identity.Application.Ports;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>Gibt alle User zurück (inkl. inaktive) – nur für Master-Verwaltung.</summary>
    Task<IReadOnlyList<User>> GetAllForManagementAsync(CancellationToken cancellationToken = default);
}

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SignOutAsync(string userId, CancellationToken cancellationToken = default);
}

public sealed record AuthenticationResult(
    UserId UserId,
    Email Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt);

/// <summary>Träger für das generierte JWT und seinen Ablaufzeitpunkt.</summary>
public sealed record GeneratedToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    GeneratedToken GenerateToken(User user);
    Result<UserId> ValidateToken(string token);

    /// <summary>
    /// Erstellt ein kurzlebiges Pre-Auth-JWT (10 Minuten) für einen Zwischenschritt
    /// im Login-Prozess (Email-Verifizierung, 2FA-Setup oder 2FA-Validation).
    /// </summary>
    GeneratedToken GeneratePreAuthToken(UserId userId, string stage);

    /// <summary>
    /// Validiert ein Pre-Auth-Token und gibt UserId + Stage zurück.
    /// Schlägt fehl wenn der Token abgelaufen ist oder kein Pre-Auth-Token ist.
    /// </summary>
    Result<PreAuthClaims> ValidatePreAuthToken(string token);
}

/// <summary>Claims aus einem validierten Pre-Auth-Token.</summary>
public sealed record PreAuthClaims(UserId UserId, string Stage);

/// <summary>Stage-Konstanten für Pre-Auth-Tokens.</summary>
public static class PreAuthStages
{
    public const string TwoFactorSetup      = "2fa_setup";
    public const string TwoFactorValidation = "2fa_validate";
}

/// <summary>
/// Port für TOTP-Operationen (Authenticator-App / RFC 6238).
/// Implementierung in Infrastructure via OtpNet.
/// </summary>
public interface ITotpService
{
    /// <summary>Generiert einen zufälligen Base32-codierten Secret-Key.</summary>
    string GenerateSecret();

    /// <summary>Erstellt den otpauth://-URI für die Authenticator-App.</summary>
    string GetOtpAuthUri(string secret, string email, string issuer);

    /// <summary>Generiert einen QR-Code als Base64-PNG-String.</summary>
    string GenerateQrCodeBase64(string otpAuthUri);

    /// <summary>Prüft ob der angegebene TOTP-Code für das gegebene Secret gültig ist (±1 Zeitfenster Toleranz).</summary>
    bool ValidateCode(string secret, string code);
}
