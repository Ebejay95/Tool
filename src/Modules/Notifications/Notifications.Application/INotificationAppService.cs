using Notifications.Abstractions;
using SharedKernel;

namespace Notifications.Application;

/// <summary>
/// Anwendungsschicht-Interface für das Versenden von Notifications.
/// Agnostisch gegenüber UI-Frameworks (kein Blazor-Coupling).
/// Empfänger werden explizit als userId angegeben.
/// </summary>
public interface INotificationAppService
{
    /// <summary>
    /// Sendet eine Notification an einen einzelnen Benutzer.
    /// </summary>
    Task SendAsync(
        string                                userId,
        string                                title,
        string                                body,
        string                                severity     = "info",
        IEnumerable<string>?                  channels     = null,
        IEnumerable<NotificationInteraction>? interactions = null,
        CancellationToken                     ct           = default);

    /// <summary>
    /// Sendet dieselbe Notification an mehrere Benutzer (Fan-out).
    /// </summary>
    Task SendBatchAsync(
        IEnumerable<string>  userIds,
        string               title,
        string               body,
        string               severity = "info",
        IEnumerable<string>? channels = null,
        CancellationToken    ct       = default);
}
