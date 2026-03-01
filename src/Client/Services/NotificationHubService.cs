using Notifications.Application.DTOs;
using SharedKernel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App.Services;

/// <summary>
/// WASM-seitiger Notification-Service.
/// Kombiniert SignalR-Client (für Push-Nachrichten) und HTTP-Client
/// (für CRUD-Operationen auf Notifications).
/// Ersetzt NotificationClientService + NotificationUpdateService aus dem SSR-Projekt.
/// </summary>
public sealed class NotificationHubService : IAsyncDisposable
{
    private readonly TokenService    _tokenService;
    private readonly HttpClient      _http;
    private readonly NavigationManager _navigation;

    private HubConnection? _hubConnection;

    /// <summary>
    /// Wird aufgerufen wenn eine neue Notification via SignalR eintrifft.
    /// Parameter: (Titel, Body, Severity)
    /// </summary>
    public event Action<string, string, string>? OnNotificationReceived;

    /// <summary>Wird aufgerufen wenn der Notification-Zustand aktualisiert werden soll (z.B. neue Notification).</summary>
    public event Func<Task>? OnNotificationUpdated;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationHubService(
        TokenService      tokenService,
        HttpClient        http,
        NavigationManager navigation)
    {
        _tokenService = tokenService;
        _http         = http;
        _navigation   = navigation;
    }

    // ── SignalR-Verbindung ─────────────────────────────────────────────────────

    public async Task StartAsync()
    {
        if (_hubConnection is not null) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri("/hubs/notifications"), options =>
            {
                options.AccessTokenProvider = async () => await _tokenService.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NotificationPayload>("ReceiveNotification", payload =>
        {
            OnNotificationReceived?.Invoke(
                payload.Title,
                payload.Body,
                payload.Severity ?? "info");

            // Abonnenten benachrichtigen (z.B. NotificationBell zum Refresh)
            _ = Task.Run(async () =>
            {
                if (OnNotificationUpdated is not null)
                    await OnNotificationUpdated.Invoke();
            });
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch
        {
            // Verbindungsfehler ignorieren (z.B. kein Token)
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    // ── HTTP-Operationen (analog zu NotificationClientService) ────────────────

    public async Task<Result<List<NotificationDto>>> GetNotificationsAsync(
        bool includeRead    = false,
        bool includeExpired = false,
        CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var url  = $"api/v1/notifications?includeRead={includeRead}&includeExpired={includeExpired}";
            var list = await _http.GetFromJsonAsync<List<NotificationDto>>(url, JsonOptions, ct);
            return Result.Success(list ?? []);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<NotificationDto>>(
                new Error("Notification.LoadFailed", ex.Message));
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var dto = await _http.GetFromJsonAsync<UnreadCountDto>(
                "api/v1/notifications/unread-count", JsonOptions, ct);
            return Result.Success(dto?.Count ?? 0);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(new Error("Notification.CountFailed", ex.Message));
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PatchAsync(
                $"api/v1/notifications/{notificationId}/read", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> MarkAllAsReadAsync(CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.PatchAsync("api/v1/notifications/read-all", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteAsync(Guid notificationId, CancellationToken ct = default)
    {
        try
        {
            await AttachTokenAsync();
            var response = await _http.DeleteAsync($"api/v1/notifications/{notificationId}", ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    /// <summary>
    /// Löst alle registrierten Update-Handler manuell aus
    /// (ersetzt NotificationUpdateService.NotifyAsync()).
    /// </summary>
    public async Task NotifyAsync()
    {
        if (OnNotificationUpdated is not null)
            await OnNotificationUpdated.Invoke();
    }

    public void Subscribe(Func<Task> handler)   => OnNotificationUpdated += handler;
    public void Unsubscribe(Func<Task> handler)  => OnNotificationUpdated -= handler;

    // ── IAsyncDisposable ──────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────────

    private async Task AttachTokenAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    private sealed record NotificationPayload(
        string Title,
        string Body,
        string? Severity,
        DateTime Timestamp);

    private sealed record UnreadCountDto(int Count);
}
