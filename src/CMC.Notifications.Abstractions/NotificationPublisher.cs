namespace CMC.Notifications.Abstractions;

public sealed class NotificationPublisher(IEnumerable<INotificationChannel> channels) : INotificationPublisher
{
    public Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var channel = channels.FirstOrDefault(c => string.Equals(c.Name, message.Channel, StringComparison.OrdinalIgnoreCase));
        if (channel is null)
        {
            throw new InvalidOperationException($"No notification channel registered for '{message.Channel}'.");
        }

        return channel.SendAsync(message, cancellationToken);
    }
}
