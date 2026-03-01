using Notifications.Abstractions;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;
using SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    // ── IRepository<Notification> ─────────────────────────────────────────────

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == NotificationId.From(id), cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Notification entity)    => _context.Notifications.Add(entity);
    public void Update(Notification entity) => _context.Notifications.Update(entity);
    public void Remove(Notification entity) => _context.Notifications.Remove(entity);

    // ── INotificationRepository ───────────────────────────────────────────────

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        UserId            recipientId,
        bool              includeRead       = true,
        bool              includeExpired    = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.RecipientId == recipientId);

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        if (!includeExpired)
            query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountUnreadAsync(
        UserId            recipientId,
        CancellationToken cancellationToken = default)
        => await _context.Notifications
            .CountAsync(n =>
                n.RecipientId == recipientId &&
                !n.IsRead &&
                (n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow),
                cancellationToken);
}
