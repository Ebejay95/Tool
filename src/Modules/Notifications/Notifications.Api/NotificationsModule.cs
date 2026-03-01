using Notifications.Application;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Hubs;
using ServerKernel;
using SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Notifications.Api;

[ModuleOrder(100)]
public sealed class NotificationsModule : IModule, IMapModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddControllers().AddApplicationPart(typeof(NotificationsModule).Assembly);
        services.AddNotifications();
        services.AddEmailNotifications(options =>
        {
            options.SimulateOnly = environment.IsDevelopment();
            options.Host = configuration.GetValue<string>("EmailNotifications:Host") ?? "localhost";
            options.Port = configuration.GetValue<int>("EmailNotifications:Port", 8025);
        });
        services.AddSignalRNotifications();
        services.AddPersistentNotifications(configuration);
        services.AddNotificationsApplication();
        return services;
    }

    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
        => endpoints.MapHub<NotificationHub>("/hubs/notifications");
}
