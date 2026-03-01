using Notifications.Abstractions;
using SharedKernel;

namespace Notifications.Domain;

/// <summary>
/// Persistente Notification – Aggregat-Root des Notifications-Domänenmodells.
/// Wird vom "persistent"-Channel erzeugt und gespeichert.
/// Kann aus der Datenbank gelesen werden (Notification-Center, Glocken-Badge etc.).
/// </summary>
public sealed class Notification : AggregateRoot, IResourceOwner
{
    string IResourceOwner.OwnerId => RecipientId.Value.ToString();

    private readonly List<NotificationInteraction> _interactions = [];

    private Notification() { } // für EF Core

    public new NotificationId       Id          { get; private set; } = null!;
    public     UserId               RecipientId { get; private set; } = null!;
    public     string               Title       { get; private set; } = string.Empty;
    public     string               Body        { get; private set; } = string.Empty;
    public     NotificationSeverity Severity    { get; private set; }
    public     bool                 IsRead      { get; private set; }
    public     DateTime?            ExpiresAt   { get; private set; }
    public     DateTime             CreatedAt   { get; private set; }

    public IReadOnlyList<NotificationInteraction> Interactions => _interactions.AsReadOnly();

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Notification Create(
        UserId                                  recipientId,
        string                                  title,
        string                                  body,
        NotificationSeverity                    severity     = NotificationSeverity.Info,
        DateTime?                               expiresAt    = null,
        IEnumerable<NotificationInteraction>?   interactions = null)
    {
        var notification = new Notification
        {
            Id          = NotificationId.New(),
            RecipientId = recipientId,
            Title       = title,
            Body        = body,
            Severity    = severity,
            IsRead      = false,
            ExpiresAt   = expiresAt,
            CreatedAt   = DateTime.UtcNow
        };

        if (interactions is not null)
            notification._interactions.AddRange(interactions);

        return notification;
    }

    // ── Verhalten ─────────────────────────────────────────────────────────────

    /// <summary>Markiert die Notification als gelesen.</summary>
    public void MarkAsRead() => IsRead = true;
}
