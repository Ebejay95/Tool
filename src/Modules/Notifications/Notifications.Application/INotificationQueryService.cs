using Notifications.Application.DTOs;
using SharedKernel;

namespace Notifications.Application;

/// <summary>
/// Anwendungsschicht-Service für persistente Notifications (lesen + verwalten).
/// Wird direkt von Blazor-Komponenten injiziert – kein HTTP-Loopback.
/// </summary>
public interface INotificationQueryService
{
    Task<IReadOnlyList<NotificationDto>> GetByRecipientAsync(
        UserId            recipientId,
        bool              includeRead       = false,
        bool              includeExpired    = false,
        CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(
        UserId            recipientId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        Guid              notificationId,
        UserId            recipientId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAllAsReadAsync(
        UserId            recipientId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid              notificationId,
        UserId            recipientId,
        CancellationToken cancellationToken = default);
}
