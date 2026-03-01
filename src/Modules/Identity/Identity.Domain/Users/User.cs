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

    private static string GenerateResetToken()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }
}
