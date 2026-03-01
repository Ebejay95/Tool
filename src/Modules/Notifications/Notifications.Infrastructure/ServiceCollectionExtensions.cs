using Notifications.Abstractions;
using Notifications.Application;
using Notifications.Domain;
using Notifications.Infrastructure.Channels;
using Notifications.Infrastructure.Hubs;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddSingleton<INotificationPublisher, NotificationPublisher>();
        return services;
    }

    public static IServiceCollection AddEmailNotifications(
        this IServiceCollection services,
        Action<SocketEmailNotificationOptions>? configureOptions = null)
    {
        services.AddSingleton<IEmailNotificationChannel, SocketEmailNotificationChannel>();
        services.AddSingleton<INotificationChannel>(provider =>
            provider.GetRequiredService<IEmailNotificationChannel>());

        if (configureOptions != null)
            services.Configure(configureOptions);

        return services;
    }

    /// <summary>
    /// Registriert den SignalR-Channel (Snackbar/Toast im Browser).
    /// Voraussetzung: AddSignalR() wurde bereits aufgerufen.
    /// In Program.cs muss außerdem app.MapHub&lt;NotificationHub&gt;("/hubs/notifications") stehen.
    /// </summary>
    public static IServiceCollection AddSignalRNotifications(this IServiceCollection services)
    {
        services.AddSingleton<ISignalRNotificationChannel, SignalRNotificationChannel>();
        services.AddSingleton<INotificationChannel>(provider =>
            provider.GetRequiredService<ISignalRNotificationChannel>());

        return services;
    }

    /// <summary>
    /// Registriert den persistenten Notification-Channel inkl. eigenem DbContext.
    /// Aktiviert das Speichern von Notifications in der Datenbank wenn "persistent"
    /// im Channels-Array einer <see cref="NotificationMessage"/> enthalten ist.
    /// </summary>
    public static IServiceCollection AddPersistentNotifications(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        var connectionString =
            configuration.GetConnectionString("NotificationsDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Notifications database connection string not found");

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.FullName)));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();

        // Singleton-Channel nutzt IServiceScopeFactory für den scoped DbContext
        services.AddSingleton<INotificationChannel, PersistentNotificationChannel>();

        return services;
    }

    public static async Task<IServiceProvider> MigrateNotificationsDatabaseAsync(
        this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        await context.Database.MigrateAsync();
        return serviceProvider;
    }
}
