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

    /// <summary>
    /// Initialisiert den Auth-Zustand aus dem Browser-LocalStorage (nach Page-Load).
    /// </summary>
    Task InitializeAsync();
}
