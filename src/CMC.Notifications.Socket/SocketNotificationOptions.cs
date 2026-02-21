namespace CMC.Notifications.Socket;

public sealed class SocketNotificationOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 9000;
    public int ConnectTimeoutMs { get; set; } = 2_000;
    public int WriteTimeoutMs { get; set; } = 2_000;
}
