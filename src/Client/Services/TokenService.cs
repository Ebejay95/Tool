using Microsoft.JSInterop;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// Verwaltet das JWT in localStorage via IJSRuntime.
/// Kein MediatR, kein Server-Context – läuft vollständig im Browser.
/// </summary>
public sealed class TokenService
{
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "auth_token";

    public TokenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Holt das Token aus localStorage und prüft ob es noch gültig ist.
    /// Gibt null zurück wenn kein Token vorhanden oder abgelaufen.
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            if (string.IsNullOrEmpty(token)) return null;

            if (IsTokenExpired(token))
            {
                await RemoveTokenAsync();
                return null;
            }

            return token;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Speichert das JWT in localStorage.</summary>
    public async Task SetTokenAsync(string token)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }
        catch
        {
            // JSInterop ist in WASM immer verfügbar, aber sicherheitshalber abfangen
        }
    }

    /// <summary>Entfernt das JWT aus localStorage.</summary>
    public async Task RemoveTokenAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
        catch { }
    }

    /// <summary>
    /// Parst den JWT Payload und gibt UserInfo zurück.
    /// </summary>
    public async Task<Identity.Application.DTOs.UserInfo?> GetCurrentUserAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var payload = ParseJwtPayload(token);
            if (payload is null) return null;

            var userId    = payload.TryGetValue("sub",                out var sub)   ? sub.ToString()   : null;
            var email     = payload.TryGetValue("email",              out var em)    ? em.ToString()     : null;
            var firstName = payload.TryGetValue("given_name",         out var fn)    ? fn.ToString()     : null;
            var lastName  = payload.TryGetValue("family_name",        out var ln)    ? ln.ToString()     : null;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return null;

            return new Identity.Application.DTOs.UserInfo
            {
                Id        = userId,
                Email     = email ?? string.Empty,
                FirstName = firstName ?? string.Empty,
                LastName  = lastName ?? string.Empty,
                FullName  = $"{firstName} {lastName}".Trim()
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Liest den 'sub'-Claim (User-ID) aus dem JWT-Token.</summary>
    public string? ExtractUserIdFromToken(string token)
    {
        try
        {
            var payload = ParseJwtPayload(token);
            return payload?.TryGetValue("sub", out var sub) == true ? sub.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Liest die Rolle des eingeloggten Users aus dem JWT ('role'-Claim).</summary>
    public async Task<string?> GetCurrentRoleAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var payload = ParseJwtPayload(token);
            if (payload is null) return null;
            return payload.TryGetValue("role", out var role) ? role.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    // ── Interne Hilfsmethoden ────────────────────────────────────────────────

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var payload = ParseJwtPayload(token);
            if (payload is null) return true;

            if (!payload.TryGetValue("exp", out var expObj)) return false;

            var exp = expObj is JsonElement el ? el.GetInt64() : Convert.ToInt64(expObj);
            var expiry = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            return DateTime.UtcNow >= expiry;
        }
        catch
        {
            return true;
        }
    }

    private static Dictionary<string, object>? ParseJwtPayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return null;

        // Base64Url → Base64
        var payloadBase64 = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');

        switch (payloadBase64.Length % 4)
        {
            case 2: payloadBase64 += "=="; break;
            case 3: payloadBase64 += "=";  break;
        }

        var payloadBytes = Convert.FromBase64String(payloadBase64);
        var payloadJson  = System.Text.Encoding.UTF8.GetString(payloadBytes);

        return JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
    }
}
