using SharedKernel;

namespace Notifications.Abstractions;

public sealed class NotificationMessage
{
    public string[]                                Channels     { get; init; }
    public string                                  Recipient    { get; init; }
    public string                                  Title        { get; init; }
    public string                                  Body         { get; init; }
    public NotificationSeverity                    Severity     { get; init; }
    public DateTime?                               ExpiresAt    { get; init; }
    public IReadOnlyList<NotificationInteraction>? Interactions { get; init; }
    public Dictionary<string, object>?             Metadata     { get; init; }

    /// <summary>Multi-Channel-Konstruktor.</summary>
    public NotificationMessage(
        string[]                                channels,
        string                                  recipient,
        string                                  title,
        string                                  body,
        NotificationSeverity                    severity     = NotificationSeverity.Info,
        DateTime?                               expiresAt    = null,
        IReadOnlyList<NotificationInteraction>? interactions = null,
        Dictionary<string, object>?             metadata     = null)
    {
        Channels     = channels;
        Recipient    = recipient;
        Title        = title;
        Body         = body;
        Severity     = severity;
        ExpiresAt    = expiresAt;
        Interactions = interactions;
        Metadata     = metadata;
    }

    /// <summary>Single-Channel-Convenience-Konstruktor.</summary>
    public NotificationMessage(
        string                                  channel,
        string                                  recipient,
        string                                  title,
        string                                  body,
        NotificationSeverity                    severity     = NotificationSeverity.Info,
        DateTime?                               expiresAt    = null,
        IReadOnlyList<NotificationInteraction>? interactions = null,
        Dictionary<string, object>?             metadata     = null)
        : this(new[] { channel }, recipient, title, body, severity, expiresAt, interactions, metadata) { }
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Delivered
}

public sealed record NotificationResult(
    string             Channel,
    NotificationStatus Status,
    string?            ErrorMessage = null,
    DateTime?          SentAt       = null);

public interface INotificationChannel
{
    string ChannelName { get; }
    Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}

public interface INotificationPublisher
{
    Task<Result<List<NotificationResult>>> PublishAsync(
        NotificationMessage message,
        CancellationToken   cancellationToken = default);

    Task<Result<List<NotificationResult>>> PublishBatchAsync(
        IEnumerable<NotificationMessage> messages,
        CancellationToken                cancellationToken = default);
}

// Specific channel types
public interface IEmailNotificationChannel : INotificationChannel
{
}

public interface ISmsNotificationChannel : INotificationChannel
{
}

public interface ISignalRNotificationChannel : INotificationChannel
{
}

// Channel names
public static class NotificationChannels
{
    public const string Email      = "email";
    public const string Sms        = "sms";
    public const string SignalR    = "signalr";
    /// <summary>
    /// Persistiert die Notification in der Datenbank.
    /// Kann mit anderen Channels kombiniert werden,
    /// z. B. ["signalr", "persistent"] oder ["email", "persistent"].
    /// </summary>
    public const string Persistent = "persistent";
}

// ── Persistente Notification-Entity und INotificationRepository ──────────────
// Wurden nach Notifications.Domain verschoben.
// using Notifications.Domain; verwenden.
