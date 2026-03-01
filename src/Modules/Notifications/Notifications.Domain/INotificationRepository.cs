using SharedKernel;

namespace Notifications.Domain;

/// <summary>
/// Repository-Interface für persistente Notifications.
/// Implementierung liegt in Notifications.Infrastructure.
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        UserId            recipientId,
        bool              includeRead       = true,
        bool              includeExpired    = false,
        CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(
        UserId            recipientId,
        CancellationToken cancellationToken = default);
}
