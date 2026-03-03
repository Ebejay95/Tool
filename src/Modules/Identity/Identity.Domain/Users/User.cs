using SharedKernel;

namespace Identity.Domain.Users;

public sealed class User : AggregateRoot
{
    private User() { } // For EF

    private User(UserId userId, Email email, HashedPassword hashedPassword, string firstName, string lastName)
    {
        Id = userId;
        Email = email;
        PasswordHash = hashedPassword;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsEmailVerified = false;

        AddDomainEvent(new UserRegisteredEvent(userId, email));
    }

    public new UserId Id { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public HashedPassword PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // ── E-Mail-Verifizierung ─────────────────────────────────────────────────
    public bool IsEmailVerified { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    /// <summary>Zeitpunkt, zu dem zuletzt eine Verifizierungs-E-Mail versendet wurde (Rate-Limit: 1×/Stunde).</summary>
    public DateTime? EmailVerificationLastSentAt { get; private set; }

    // ── Zwei-Faktor-Authentifizierung (TOTP / Authenticator-App) ────────────
    /// <summary>Bestätigtes TOTP-Geheimnis (Base32). Nur gesetzt wenn 2FA aktiv ist.</summary>
    public string? TwoFactorSecret { get; private set; }
    /// <summary>Noch nicht bestätigtes TOTP-Geheimnis während des Setup-Vorgangs.</summary>
    public string? TwoFactorPendingSecret { get; private set; }
    public bool IsTwoFactorEnabled { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public static Result<User> Create(Email email, string password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<User>(UserErrors.PasswordRequired);

        if (password.Length < 8)
            return Result.Failure<User>(UserErrors.PasswordTooShort);

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<User>(UserErrors.FirstNameRequired);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<User>(UserErrors.LastNameRequired);

        var hashedPassword = HashedPassword.Create(password);
        var userId = UserId.New();

        return Result.Success(new User(userId, email, hashedPassword, firstName.Trim(), lastName.Trim()));
    }

    public bool VerifyPassword(string password)
    {
        return PasswordHash.Verify(password);
    }

    public Result ChangePassword(string currentPassword, string newPassword)
    {
        if (!VerifyPassword(currentPassword))
            return Result.Failure(UserErrors.InvalidCurrentPassword);

        if (newPassword.Length < 8)
            return Result.Failure(UserErrors.PasswordTooShort);

        PasswordHash = HashedPassword.Create(newPassword);
        AddDomainEvent(new UserPasswordChangedEvent(Id, Email));

        return Result.Success();
    }

    public Result InitiatePasswordReset()
    {
        PasswordResetToken = GenerateResetToken();
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        AddDomainEvent(new UserPasswordResetRequestedEvent(Id, Email, PasswordResetToken));

        return Result.Success();
    }

    public Result ResetPassword(string token, string newPassword)
    {
        if (string.IsNullOrEmpty(PasswordResetToken) || PasswordResetToken != token)
            return Result.Failure(UserErrors.InvalidResetToken);

        if (PasswordResetTokenExpiry == null || PasswordResetTokenExpiry < DateTime.UtcNow)
            return Result.Failure(UserErrors.ExpiredResetToken);

        if (newPassword.Length < 8)
            return Result.Failure(UserErrors.PasswordTooShort);

        PasswordHash = HashedPassword.Create(newPassword);
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;

        AddDomainEvent(new UserPasswordResetCompletedEvent(Id, Email));

        return Result.Success();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        AddDomainEvent(new UserLoggedInEvent(Id, Email));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id, Email));
    }

    // ── E-Mail-Verifizierung ─────────────────────────────────────────────────

    /// <summary>
    /// Generiert ein neues Verifizierungstoken und feuert das Domain-Event.
    /// Rate-Limit: maximal einmal pro Stunde.
    /// </summary>
    public Result RequestEmailVerification()
    {
        if (IsEmailVerified)
            return Result.Failure(UserErrors.EmailAlreadyVerified);

        if (EmailVerificationLastSentAt.HasValue &&
            (DateTime.UtcNow - EmailVerificationLastSentAt.Value).TotalHours < 1)
            return Result.Failure(UserErrors.EmailVerificationTooSoon);

        EmailVerificationToken = GenerateResetToken();
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        EmailVerificationLastSentAt = DateTime.UtcNow;

        AddDomainEvent(new UserEmailVerificationRequestedEvent(Id, Email, EmailVerificationToken));

        return Result.Success();
    }

    /// <summary>Bestätigt die E-Mail-Adresse anhand des zugesandten Tokens.</summary>
    public Result VerifyEmail(string token)
    {
        if (IsEmailVerified)
            return Result.Failure(UserErrors.EmailAlreadyVerified);

        if (string.IsNullOrEmpty(EmailVerificationToken) || EmailVerificationToken != token)
            return Result.Failure(UserErrors.InvalidEmailVerificationToken);

        if (EmailVerificationTokenExpiry == null || EmailVerificationTokenExpiry < DateTime.UtcNow)
            return Result.Failure(UserErrors.ExpiredEmailVerificationToken);

        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;

        AddDomainEvent(new UserEmailVerifiedEvent(Id, Email));

        return Result.Success();
    }

    // ── Zwei-Faktor-Authentifizierung ────────────────────────────────────────

    /// <summary>
    /// Speichert ein vom Application-Layer generiertes TOTP-Geheimnis als "pending"
    /// (noch nicht durch einen gültigen Code bestätigt).
    /// </summary>
    public Result InitiateTwoFactorSetup(string pendingSecret)
    {
        if (IsTwoFactorEnabled)
            return Result.Failure(UserErrors.TwoFactorAlreadyEnabled);

        TwoFactorPendingSecret = pendingSecret;
        return Result.Success();
    }

    /// <summary>
    /// Bestätigt die 2FA-Einrichtung. Der übergebene <paramref name="codeValid"/>-Parameter
    /// wird vom Application-Layer mit dem TOTP-Service validiert.
    /// </summary>
    public Result ConfirmTwoFactorSetup(bool codeValid)
    {
        if (string.IsNullOrEmpty(TwoFactorPendingSecret))
            return Result.Failure(UserErrors.TwoFactorNotConfigured);

        if (!codeValid)
            return Result.Failure(UserErrors.InvalidTwoFactorCode);

        TwoFactorSecret = TwoFactorPendingSecret;
        TwoFactorPendingSecret = null;
        IsTwoFactorEnabled = true;

        AddDomainEvent(new UserTwoFactorEnabledEvent(Id, Email));

        return Result.Success();
    }

    /// <summary>Deaktiviert 2FA (z.B. beim Account-Reset).</summary>
    public Result DisableTwoFactor()
    {
        if (!IsTwoFactorEnabled)
            return Result.Failure(UserErrors.TwoFactorNotConfigured);

        TwoFactorSecret = null;
        TwoFactorPendingSecret = null;
        IsTwoFactorEnabled = false;

        return Result.Success();
    }

    private static string GenerateResetToken()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }
}
