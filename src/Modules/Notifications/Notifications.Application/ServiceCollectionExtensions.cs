using Microsoft.Extensions.DependencyInjection;

namespace Notifications.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registriert die Notifications-Application-Services.
    /// Setzt voraus, dass <c>AddNotifications()</c> (Infrastructure) bereits
    /// aufgerufen wurde, damit <c>INotificationPublisher</c> verfügbar ist.
    /// </summary>
    public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationAppService, NotificationAppService>();
        return services;
    }
}
