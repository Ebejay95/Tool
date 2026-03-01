using Notifications.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Notifications.Infrastructure.Channels;

public sealed class SocketEmailNotificationOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8025;
    public int TimeoutMs { get; set; } = 5000;
    public bool SimulateOnly { get; set; } = false; // For development/testing
}

public sealed class SocketEmailNotificationChannel : IEmailNotificationChannel
{
    private readonly SocketEmailNotificationOptions _options;
    private readonly ILogger<SocketEmailNotificationChannel> _logger;

    public string ChannelName => NotificationChannels.Email;

    public SocketEmailNotificationChannel(
        IOptions<SocketEmailNotificationOptions> options,
        ILogger<SocketEmailNotificationChannel> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<NotificationResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        if (_options.SimulateOnly)
        {
            return await SimulateSendAsync(message);
        }

        try
        {
            using var client = new TcpClient();
            using var timeoutCts = new CancellationTokenSource(_options.TimeoutMs);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await client.ConnectAsync(_options.Host, _options.Port, combinedCts.Token);

            var emailData = new
            {
                To       = message.Recipient,
                Subject  = message.Title,
                Body     = message.Body,
                Channel  = ChannelName,
                Metadata = message.Metadata,
                Timestamp = DateTime.UtcNow
            };

            var jsonData = JsonSerializer.Serialize(emailData);
            var data = Encoding.UTF8.GetBytes(jsonData + "\n");

            var stream = client.GetStream();
            await stream.WriteAsync(data, combinedCts.Token);
            await stream.FlushAsync(combinedCts.Token);

            _logger.LogInformation(
                "Email notification sent via socket to {Host}:{Port} for {Recipient}",
                _options.Host, _options.Port, message.Recipient);

            return new NotificationResult(ChannelName, NotificationStatus.Sent, null, DateTime.UtcNow);
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "Socket error sending email to {Recipient}", message.Recipient);
            return new NotificationResult(ChannelName, NotificationStatus.Failed, $"Socket error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Email send timeout for {Recipient}", message.Recipient);
            return new NotificationResult(ChannelName, NotificationStatus.Failed, "Send timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Recipient}", message.Recipient);
            return new NotificationResult(ChannelName, NotificationStatus.Failed, $"Unexpected error: {ex.Message}");
        }
    }

    private async Task<NotificationResult> SimulateSendAsync(NotificationMessage message)
    {
        // Simulate network delay
        await Task.Delay(100);

        _logger.LogInformation(
            "[SIMULATED] Email would be sent to {Recipient} with title '{Title}'",
            message.Recipient, message.Title);

        return new NotificationResult(ChannelName, NotificationStatus.Sent, null, DateTime.UtcNow);
    }
}
