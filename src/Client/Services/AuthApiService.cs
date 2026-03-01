using Identity.Application;
using Identity.Application.DTOs;
using SharedKernel;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// WASM-Implementierung von IAuthService.
/// Alle Operationen laufen über HTTP-API-Aufrufe + TokenService für localStorage.
/// Kein MediatR, kein CircuitTokenHolder, kein Server-Context.
/// </summary>
public sealed class AuthApiService : IAuthService
{
    private readonly HttpClient  _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthApiService(HttpClient http, TokenService tokenService)
    {
        _http         = http;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure<LoginResponseDto>(
                    new Error(error.Code, error.Message));
            }

            var loginResponse = await response.Content
                .ReadFromJsonAsync<LoginResponseDto>(JsonOptions);

            if (loginResponse is null || string.IsNullOrEmpty(loginResponse.Token))
                return Result.Failure<LoginResponseDto>(
                    new Error("Auth.InvalidResponse", "Ungültige Server-Antwort beim Login."));

            await _tokenService.SetTokenAsync(loginResponse.Token);
            return Result.Success(loginResponse);
        }
        catch (Exception ex)
        {
            return Result.Failure<LoginResponseDto>(
                new Error("Auth.NetworkError", ex.Message));
        }
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterUserDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/v1/auth/register", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure<UserDto>(new Error(error.Code, error.Message));
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
            if (user is null)
                return Result.Failure<UserDto>(
                    new Error("Auth.InvalidResponse", "Ungültige Server-Antwort bei Registrierung."));

            return Result.Success(user);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDto>(new Error("Auth.NetworkError", ex.Message));
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public Task<string?> GetTokenAsync() => _tokenService.GetTokenAsync();

    public Task<UserInfo?> GetCurrentUserAsync() => _tokenService.GetCurrentUserAsync();

    public async Task LogoutAsync()
    {
        await _tokenService.RemoveTokenAsync();
    }

    public async Task<Result> RequestPasswordResetAsync(string email)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/v1/auth/request-password-reset",
                new { Email = email });

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(error.Code, error.Message));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Auth.NetworkError", ex.Message));
        }
    }

    public async Task<Result> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/v1/auth/reset-password",
                new ResetPasswordDto { Token = token, NewPassword = newPassword });

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadApiErrorAsync(response);
                return Result.Failure(new Error(error.Code, error.Message));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Auth.NetworkError", ex.Message));
        }
    }

    public async Task InitializeAsync()
    {
        // Token aus localStorage laden – läuft in WASM automatisch via TokenService.
        // Keine explizite Initialisierung nötig, GetTokenAsync prüft bereits den Ablauf.
        await _tokenService.GetTokenAsync();
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────────

    private static async Task<(string Code, string Message)> TryReadApiErrorAsync(
        HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var code    = doc.RootElement.TryGetProperty("error",   out var c)
                        ? c.GetString() ?? "Api.Error"
                        : "Api.Error";
            var message = doc.RootElement.TryGetProperty("message", out var m)
                        ? m.GetString() ?? response.ReasonPhrase ?? "Unbekannter Fehler"
                        : response.ReasonPhrase ?? "Unbekannter Fehler";
            return (code, message);
        }
        catch
        {
            return ("Api.Error", $"HTTP {(int)response.StatusCode}");
        }
    }
}
