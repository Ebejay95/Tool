using Identity.Application.DTOs;
using SharedKernel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// Nutzerverwaltung für den Master-User.
/// Kommuniziert per HTTP mit /api/v1/users.
/// </summary>
public sealed class UserManagementApiService
{
    private readonly HttpClient   _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserManagementApiService(HttpClient http, TokenService tokenService)
    {
        _http         = http;
        _tokenService = tokenService;
    }

    public async Task<Result<List<UserManagementItemDto>>> GetAllUsersAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var users = await _http.GetFromJsonAsync<List<UserManagementItemDto>>(
                "api/v1/users", JsonOptions, ct);
            return Result.Success(users ?? []);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserManagementItemDto>>(new Error("Users.LoadFailed", ex.Message));
        }
    }

    public async Task<Result> UpdateUserRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var dto = new UpdateUserRoleDto(role);
            var response = await _http.PutAsJsonAsync($"api/v1/users/{userId}/role", dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(body);
                var message = doc.RootElement.TryGetProperty("message", out var m)
                    ? m.GetString() ?? "Fehler beim Speichern"
                    : "Fehler beim Speichern";
                return Result.Failure(new Error("Users.UpdateRoleFailed", message));
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Users.UpdateRoleFailed", ex.Message));
        }
    }

    private async Task AttachTokenAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }
}
