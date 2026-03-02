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

public sealed class LoginResponseDto
{
    public string UserId    { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Token     { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public sealed class UserInfo
{
    public string Id        { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string FullName  { get; set; } = string.Empty;
}
