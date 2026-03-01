using Notifications.Abstractions;
using SharedKernel;
using Microsoft.Extensions.Logging;

namespace Notifications.Infrastructure;

public sealed class NotificationPublisher : INotificationPublisher
{
    private readonly Dictionary<string, INotificationChannel> _channelLookup;
    private readonly ILogger<NotificationPublisher> _logger;

    public NotificationPublisher(
        IEnumerable<INotificationChannel> channels,
        ILogger<NotificationPublisher>    logger)
    {
        _logger        = logger;
        _channelLookup = channels.ToDictionary(
            c => c.ChannelName,
            c => c,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Fan-out an alle in <see cref="NotificationMessage.Channels"/> angegebenen Channels.
    /// </summary>
    public async Task<Result<List<NotificationResult>>> PublishAsync(
        NotificationMessage message,
        CancellationToken   cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Channels.Length == 0)
        {
            _logger.LogWarning("PublishAsync ohne Channels für Recipient={Recipient}", message.Recipient);
            return Result.Failure<List<NotificationResult>>(
                new Error("Notification.NoChannels", "Keine Channels angegeben."));
        }

        var results = new List<NotificationResult>(message.Channels.Length);

        foreach (var channelName in message.Channels)
        {
            if (!_channelLookup.TryGetValue(channelName, out var channel))
            {
                _logger.LogError(
                    "Kein Channel registriert für '{Channel}' – übersprungen (Recipient={Recipient})",
                    channelName, message.Recipient);

                results.Add(new NotificationResult(
                    channelName,
                    NotificationStatus.Failed,
                    $"Channel '{channelName}' ist nicht konfiguriert"));
                continue;
            }

            try
            {
                var result = await channel.SendAsync(message, cancellationToken);
                results.Add(result);

                _logger.LogInformation(
                    "Notification via Channel={Channel} an Recipient={Recipient}: Status={Status}",
                    channelName, message.Recipient, result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fehler beim Senden via Channel={Channel} an Recipient={Recipient}",
                    channelName, message.Recipient);

                results.Add(new NotificationResult(
                    channelName,
                    NotificationStatus.Failed,
                    ex.Message));
            }
        }

        return Result.Success(results);
    }

    public async Task<Result<List<NotificationResult>>> PublishBatchAsync(
        IEnumerable<NotificationMessage> messages,
        CancellationToken                cancellationToken = default)
    {
        var allResults = new List<NotificationResult>();

        foreach (var message in messages)
        {
            var result = await PublishAsync(message, cancellationToken);
            if (result.IsSuccess)
                allResults.AddRange(result.Value);
        }

        return Result.Success(allResults);
    }
}
