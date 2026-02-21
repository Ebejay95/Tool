using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CMC.Notifications.Abstractions;
using Microsoft.Extensions.Options;

namespace CMC.Notifications.Socket;

public sealed class SocketNotificationChannel(IOptions<SocketNotificationOptions> options) : INotificationChannel
{
    public string Name => "email";

    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var config = options.Value;

        using var client = new TcpClient();

        using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            connectCts.CancelAfter(TimeSpan.FromMilliseconds(config.ConnectTimeoutMs));
            await client.ConnectAsync(config.Host, config.Port, connectCts.Token);
        }

        client.SendTimeout = config.WriteTimeoutMs;
        client.ReceiveTimeout = config.WriteTimeoutMs;

        await using var stream = client.GetStream();

        var payload = JsonSerializer.Serialize(new
        {
            channel = message.Channel,
            to = message.To,
            subject = message.Subject,
            body = message.Body,
            metadata = message.Metadata
        });

        var bytes = Encoding.UTF8.GetBytes(payload + "\n");
        await stream.WriteAsync(bytes, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}
