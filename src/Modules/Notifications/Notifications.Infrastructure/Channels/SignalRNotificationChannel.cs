using Notifications.Abstractions;
using Notifications.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Notifications.Infrastructure.Channels;

/// <summary>
/// Sendet Notifications via SignalR an den verbundenen Browser-Tab des Nutzers.
/// Die MainLayout-Komponente empfängt sie und zeigt sie als MudBlazor-Snackbar an.
/// </summary>
public sealed class SignalRNotificationChannel : ISignalRNotificationChannel
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationChannel> _logger;

    public string ChannelName => NotificationChannels.SignalR;

    public SignalRNotificationChannel(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationChannel> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var severity = message.Metadata?.TryGetValue("severity", out var s) == true
                ? s?.ToString() ?? "info"
                : "info";

            await _hubContext.Clients
                .User(message.Recipient)
                .SendAsync("ReceiveNotification", new
                {
                    message.Title,
                    message.Body,
                    Severity = severity,
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);

            _logger.LogInformation(
                "SignalR notification sent to user {UserId}: {Title}",
                message.Recipient, message.Title);

            return new NotificationResult(ChannelName, NotificationStatus.Sent, SentAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification to user {UserId}", message.Recipient);
            return new NotificationResult(ChannelName, NotificationStatus.Failed, ex.Message);
        }
    }
}
