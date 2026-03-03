namespace Identity.Application.DTOs;

public sealed class RegisterUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed record RequestPasswordResetDto(
    string Email);

public sealed class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool IsActive);

public sealed record AuthenticationDto(
    string Token,
    UserDto User,
    DateTime ExpiresAt);

// ── Auth-Antwort-DTOs (werden vom AuthController zurückgegeben) ───────────────

/// <summary>
/// Phase des mehrstufigen Login-Vorgangs.
/// </summary>
public enum LoginStage
{
    /// <summary>Login abgeschlossen – JWT-Token ausgestellt.</summary>
    Complete = 0,
    /// <summary>E-Mail-Adresse wurde noch nicht bestätigt.</summary>
    RequiresEmailVerification = 1,
    /// <summary>2FA ist noch nicht eingerichtet – Setup erforderlich.</summary>
    RequiresTwoFactorSetup = 2,
    /// <summary>2FA ist eingerichtet – TOTP-Code erforderlich.</summary>
    RequiresTwoFactorValidation = 3
}

/// <summary>
/// Antwort auf einen Login-Versuch. Bei <see cref="LoginStage.Complete"/> sind
/// <see cref="Token"/> und <see cref="ExpiresAt"/> gefüllt. Bei allen anderen
/// Stages enthält <see cref="PreAuthToken"/> ein kurzlebiges Pre-Auth-JWT (10 min).
/// </summary>
public sealed class LoginResponseDto
{
    public LoginStage Stage { get; set; } = LoginStage.Complete;

    // Stage == Complete
    public string? UserId    { get; set; }
    public string? Email     { get; set; }
    public string? FirstName { get; set; }
    public string? LastName  { get; set; }
    public string? Token     { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Stage != Complete – kurzlebiges Pre-Auth-Token für den nächsten Schritt
    public string? PreAuthToken { get; set; }
}

public sealed class UserInfo
{
    public string Id        { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string FullName  { get; set; } = string.Empty;
}

// ── E-Mail-Verifizierung ──────────────────────────────────────────────────────

public sealed record VerifyEmailDto(string Email, string Token);

public sealed record ResendVerificationEmailDto(string Email);

// ── Zwei-Faktor-Authentifizierung ─────────────────────────────────────────────

/// <summary>Antwort auf Setup-Init: Enthält den TOTP-Secret-Key und einen QR-Code (Base64-PNG).</summary>
public sealed class TwoFactorSetupDto
{
    /// <summary>Base32-codierter Secret-Key (für manuelle Eingabe in die Authenticator-App).</summary>
    public string SecretKey { get; set; } = string.Empty;
    /// <summary>QR-Code als Base64-kodiertes PNG-Bild.</summary>
    public string QrCodeBase64 { get; set; } = string.Empty;
    /// <summary>Der vollständige otpauth://-URI für die Authenticator-App.</summary>
    public string OtpAuthUri { get; set; } = string.Empty;
}

public sealed record ConfirmTwoFactorDto(string PreAuthToken, string Code);

public sealed record ValidateTwoFactorDto(string PreAuthToken, string Code);
