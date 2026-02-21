namespace CMC.Notifications.Abstractions;

public interface INotificationChannel
{
    string Name { get; }

    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
