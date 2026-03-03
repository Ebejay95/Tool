using SharedKernel;

namespace Identity.Domain.Users;

public static class UserErrors
{
    public static readonly Error EmailRequired = new("User.EmailRequired", "Email is required");
    public static readonly Error EmailInvalid = new("User.EmailInvalid", "Email format is invalid");
    public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "Email already exists");

    public static readonly Error PasswordRequired = new("User.PasswordRequired", "Password is required");
    public static readonly Error PasswordTooShort = new("User.PasswordTooShort", "Password must be at least 8 characters");
    public static readonly Error InvalidCurrentPassword = new("User.InvalidCurrentPassword", "Current password is incorrect");

    public static readonly Error FirstNameRequired = new("User.FirstNameRequired", "First name is required");
    public static readonly Error LastNameRequired = new("User.LastNameRequired", "Last name is required");

    public static readonly Error UserNotFound = new("User.NotFound", "User was not found");
    public static readonly Error UserInactive = new("User.Inactive", "User account is inactive");

    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password");

    public static readonly Error InvalidResetToken = new("User.InvalidResetToken", "Invalid password reset token");
    public static readonly Error ExpiredResetToken = new("User.ExpiredResetToken", "Password reset token has expired");

    // ── E-Mail-Verifizierung ──────────────────────────────────────────────────
    public static readonly Error EmailNotVerified = new("User.EmailNotVerified", "Email address has not been verified");
    public static readonly Error EmailAlreadyVerified = new("User.EmailAlreadyVerified", "Email address is already verified");
    public static readonly Error InvalidEmailVerificationToken = new("User.InvalidEmailVerificationToken", "Invalid email verification token");
    public static readonly Error ExpiredEmailVerificationToken = new("User.ExpiredEmailVerificationToken", "Email verification token has expired");
    public static readonly Error EmailVerificationTooSoon = new("User.EmailVerificationTooSoon", "Please wait at least one hour before requesting a new verification email");

    // ── Zwei-Faktor-Authentifizierung ─────────────────────────────────────────
    public static readonly Error TwoFactorRequired = new("User.TwoFactorRequired", "Two-factor authentication code is required");
    public static readonly Error TwoFactorAlreadyEnabled = new("User.TwoFactorAlreadyEnabled", "Two-factor authentication is already enabled");
    public static readonly Error TwoFactorNotConfigured = new("User.TwoFactorNotConfigured", "Two-factor authentication is not configured");
    public static readonly Error InvalidTwoFactorCode = new("User.InvalidTwoFactorCode", "Invalid two-factor authentication code");
}
