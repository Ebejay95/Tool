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
}
