using Notifications.Abstractions;
using SharedKernel;
using Microsoft.Extensions.Logging;

namespace Notifications.Application;

/// <summary>
/// Anwendungsschicht-Implementierung. Kapselt <see cref="INotificationPublisher"/>
/// und bietet eine domänenneutrale API ohne UI-Framework-Abhängigkeiten.
/// </summary>
internal sealed class NotificationAppService : INotificationAppService
{
    private readonly INotificationPublisher            _publisher;
    private readonly ILogger<NotificationAppService>   _logger;

    public NotificationAppService(
        INotificationPublisher          publisher,
        ILogger<NotificationAppService> logger)
    {
        _publisher = publisher;
        _logger    = logger;
    }

    public async Task SendAsync(
        string                                userId,
        string                                title,
        string                                body,
        string                                severity     = "info",
        IEnumerable<string>?                  channels     = null,
        IEnumerable<NotificationInteraction>? interactions = null,
        CancellationToken                     ct           = default)
    {
        var resolvedChannels = channels?.ToArray() is { Length: > 0 } ch
            ? ch
            : [NotificationChannels.SignalR];

        var message = new NotificationMessage(
            channels:     resolvedChannels,
            recipient:    userId,
            title:        title,
            body:         body,
            severity:     ParseSeverity(severity),
            interactions: interactions?.ToList());

        var result = await _publisher.PublishAsync(message, ct);

        if (!result.IsSuccess)
            _logger.LogWarning(
                "Notification an '{UserId}' fehlgeschlagen: {Error}",
                userId, result.Error);
    }

    public async Task SendBatchAsync(
        IEnumerable<string>  userIds,
        string               title,
        string               body,
        string               severity = "info",
        IEnumerable<string>? channels = null,
        CancellationToken    ct       = default)
    {
        var userList = userIds.ToList();

        if (userList.Count == 0)
        {
            _logger.LogWarning("SendBatchAsync: Keine Empfänger angegeben.");
            return;
        }

        var tasks = userList.Select(uid => SendAsync(uid, title, body, severity, channels, null, ct));
        await Task.WhenAll(tasks);
    }

    private static NotificationSeverity ParseSeverity(string severity) =>
        severity.ToLowerInvariant() switch
        {
            "success" => NotificationSeverity.Success,
            "warning" => NotificationSeverity.Warning,
            "error"   => NotificationSeverity.Error,
            "notice"  => NotificationSeverity.Notice,
            _         => NotificationSeverity.Info
        };
}
