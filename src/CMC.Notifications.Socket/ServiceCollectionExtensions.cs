using CMC.Notifications.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Notifications.Socket;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSocketEmailNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SocketNotificationOptions>(configuration.GetSection("Notifications:Socket"));
        services.AddSingleton<INotificationChannel, SocketNotificationChannel>();
        services.AddSingleton<INotificationPublisher, NotificationPublisher>();
        return services;
    }
}
