using Notifications.Application;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Hubs;
using ServerKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Notifications.Api;

[ModuleOrder(100)]
public sealed class NotificationsModule : IModule, IMapModule, IMigrateModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddNotifications();
        services.AddEmailNotifications(configuration);
        services.AddSignalRNotifications(configuration);
        services.AddPersistentNotifications(configuration);
        services.AddNotificationsApplication();
        return services;
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
        => endpoints.MapHub<NotificationHub>("/hubs/notifications");

    public static Task MigrateAsync(IServiceProvider serviceProvider)
        => serviceProvider.MigrateNotificationsDatabaseAsync();
}
