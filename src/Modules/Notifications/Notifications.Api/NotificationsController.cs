using Notifications.Domain;
using Notifications.Infrastructure.Persistence;
using SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notifications.Api;

// ── Response-DTOs ─────────────────────────────────────────────────────────────

public sealed record NotificationInteractionDto(
    Guid    InteractionId,
    string  Label,
    string  Type,
    string? Href,
    string? ActionName);

public sealed record NotificationDto(
    Guid                                     Id,
    string                                   Title,
    string                                   Body,
    string                                   Severity,
    bool                                     IsRead,
    DateTime?                                ExpiresAt,
    DateTime                                 CreatedAt,
    IReadOnlyList<NotificationInteractionDto> Interactions);

public sealed record UnreadCountDto(int Count);

// ── Controller ────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _repository;
    private readonly NotificationsDbContext  _dbContext;
    private readonly ICurrentUser            _currentUser;

    public NotificationsController(
        INotificationRepository repository,
        NotificationsDbContext  dbContext,
        ICurrentUser            currentUser)
    {
        _repository  = repository;
        _dbContext   = dbContext;
        _currentUser = currentUser;
    }

    // GET /api/notifications?includeRead=false&includeExpired=false
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool includeRead        = false,
        [FromQuery] bool includeExpired     = false,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var notifications = await _repository.GetByRecipientAsync(
            _currentUser.UserId, includeRead, includeExpired, cancellationToken);

        return Ok(notifications.Select(Map).ToList());
    }

    // GET /api/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var count = await _repository.CountUnreadAsync(_currentUser.UserId, cancellationToken);
        return Ok(new UnreadCountDto(count));
    }

    // PATCH /api/notifications/{id}/read
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var notification = await _repository.GetByIdAsync(id, cancellationToken);
        if (notification is null) return NotFound();

        if (notification.RecipientId.Value != _currentUser.UserId.Value) return Forbid();

        notification.MarkAsRead();
        _repository.Update(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // PATCH /api/notifications/read-all
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var unread = await _repository.GetByRecipientAsync(
            _currentUser.UserId, includeRead: false, includeExpired: false, cancellationToken);

        foreach (var n in unread)
        {
            n.MarkAsRead();
            _repository.Update(n);
        }

        if (unread.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // DELETE /api/notifications/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null) return Unauthorized();

        var notification = await _repository.GetByIdAsync(id, cancellationToken);
        if (notification is null) return NotFound();

        if (notification.RecipientId.Value != _currentUser.UserId.Value) return Forbid();

        _repository.Remove(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
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
