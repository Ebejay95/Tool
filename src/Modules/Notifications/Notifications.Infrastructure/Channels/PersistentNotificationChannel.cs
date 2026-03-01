using Notifications.Abstractions;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;
using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Notifications.Infrastructure.Channels;

/// <summary>
/// Speichert eine Notification als persistente Entity in der Datenbank.
/// Wird immer dann aktiv, wenn der Channel-Name "persistent" in
/// <see cref="NotificationMessage.Channels"/> enthalten ist.
/// <para>
/// Kann mit anderen Channels kombiniert werden, z. B.:
/// <c>["signalr", "persistent"]</c> sendet eine Echtzeit-Snackbar UND speichert die Notification.
/// </para>
/// </summary>
public sealed class PersistentNotificationChannel : INotificationChannel
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PersistentNotificationChannel> _logger;

    public string ChannelName => NotificationChannels.Persistent;

    public PersistentNotificationChannel(
        IServiceScopeFactory scopeFactory,
        ILogger<PersistentNotificationChannel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationMessage   message,
        CancellationToken     cancellationToken = default)
    {
        try
        {
            // Recipient muss eine gültige UserId (GUID) sein
            if (!Guid.TryParse(message.Recipient, out var recipientGuid))
            {
                _logger.LogWarning(
                    "PersistentNotificationChannel: Recipient '{Recipient}' ist keine gültige GUID – Notification wird nicht gespeichert.",
                    message.Recipient);

                return new NotificationResult(
                    ChannelName,
                    NotificationStatus.Failed,
                    $"Recipient '{message.Recipient}' is not a valid user ID");
            }

            var recipientId = UserId.From(recipientGuid);

            var notification = Notification.Create(
                recipientId:  recipientId,
                title:        message.Title,
                body:         message.Body,
                severity:     message.Severity,
                expiresAt:    message.ExpiresAt,
                interactions: message.Interactions);

            // Scoped DbContext – IServiceScopeFactory notwendig, da Channel Singleton ist
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Persistente Notification gespeichert: Id={Id}, Recipient={Recipient}, Title={Title}",
                notification.Id.Value, message.Recipient, message.Title);

            return new NotificationResult(
                ChannelName,
                NotificationStatus.Delivered,
                SentAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Fehler beim Speichern der persistenten Notification für Recipient={Recipient}",
                message.Recipient);

            return new NotificationResult(
                ChannelName,
                NotificationStatus.Failed,
                ex.Message);
        }
    }
}
