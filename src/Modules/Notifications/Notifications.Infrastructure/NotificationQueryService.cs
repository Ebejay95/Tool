using Notifications.Abstractions;
using Notifications.Application;
using Notifications.Application.DTOs;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;
using SharedKernel;

namespace Notifications.Infrastructure;

/// <summary>
/// Implementiert INotificationQueryService auf Basis von INotificationRepository.
/// Wird direkt von Blazor-Services injiziert – kein HTTP-Loopback mehr.
/// </summary>
internal sealed class NotificationQueryService : INotificationQueryService
{
    private readonly INotificationRepository _repository;
    private readonly NotificationsDbContext  _dbContext;

    public NotificationQueryService(
        INotificationRepository repository,
        NotificationsDbContext  dbContext)
    {
        _repository = repository;
        _dbContext  = dbContext;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetByRecipientAsync(
        UserId            recipientId,
        bool              includeRead       = false,
        bool              includeExpired    = false,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetByRecipientAsync(
            recipientId, includeRead, includeExpired, cancellationToken);

        return notifications.Select(Map).ToList();
    }

    public Task<int> CountUnreadAsync(
        UserId            recipientId,
        CancellationToken cancellationToken = default)
        => _repository.CountUnreadAsync(recipientId, cancellationToken);

    public async Task<bool> MarkAsReadAsync(
        Guid              notificationId,
        UserId            recipientId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null || notification.RecipientId.Value != recipientId.Value)
            return false;

        notification.MarkAsRead();
        _repository.Update(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(
        UserId            recipientId,
        CancellationToken cancellationToken = default)
    {
        var unread = await _repository.GetByRecipientAsync(
            recipientId, includeRead: false, includeExpired: false, cancellationToken);

        foreach (var n in unread)
        {
            n.MarkAsRead();
            _repository.Update(n);
        }

        if (unread.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid              notificationId,
        UserId            recipientId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is null || notification.RecipientId.Value != recipientId.Value)
            return false;

        _repository.Remove(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static NotificationDto Map(Notification n) => new(
        Id:           n.Id.Value,
        Title:        n.Title,
        Body:         n.Body,
        Severity:     n.Severity.ToString().ToLowerInvariant(),
        IsRead:       n.IsRead,
        ExpiresAt:    n.ExpiresAt,
        CreatedAt:    n.CreatedAt,
        Interactions: n.Interactions
            .Select(i => new NotificationInteractionDto(
                i.InteractionId,
                i.Label,
                i.Type.ToString().ToLowerInvariant(),
                i.Href,
                i.ActionName))
            .ToList());
}
