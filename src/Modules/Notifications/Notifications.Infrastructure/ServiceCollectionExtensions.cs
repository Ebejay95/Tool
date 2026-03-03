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
using StackExchange.Redis;

namespace Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddSingleton<INotificationPublisher, NotificationPublisher>();
        return services;
    }

    /// <summary>
    /// Registriert den E-Mail-Channel anhand der Konfiguration:
    ///   Email:Provider = "smtp"     → SmtpEmailNotificationChannel (Dev: MailHog, Prod: echter SMTP)
    ///   Email:Provider = "graph"    → GraphEmailNotificationChannel  (Prod: Microsoft Graph API)
    ///   Email:Provider = "simulate" → SmtpEmailNotificationChannel mit SimulateOnly=true (Standard)
    /// </summary>
    public static IServiceCollection AddEmailNotifications(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        var provider = configuration["Email:Provider"] ?? "simulate";

        if (provider.Equals("graph", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<GraphEmailSettings>(configuration.GetSection("Email:Graph"));
            services.AddSingleton<IEmailNotificationChannel, GraphEmailNotificationChannel>();
        }
        else
        {
            // smtp + simulate nutzen beide SmtpEmailNotificationChannel
            services.Configure<SmtpEmailSettings>(configuration.GetSection("Email:Smtp"));
            services.AddSingleton<IEmailNotificationChannel, SmtpEmailNotificationChannel>();
        }

        services.AddSingleton<INotificationChannel>(provider =>
            provider.GetRequiredService<IEmailNotificationChannel>());

        return services;
    }

    /// <summary>
    /// Registriert SignalR inkl. optionalem Redis-Backplane und den SignalR-Notification-Channel.
    /// Ruft AddSignalR() selbst auf – kein separater Aufruf in Program.cs nötig.
    /// </summary>
    public static IServiceCollection AddSignalRNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConn = configuration["Redis:ConnectionString"];
        var signalR = services.AddSignalR();
        if (!string.IsNullOrWhiteSpace(redisConn))
            signalR.AddStackExchangeRedis(redisConn, opts =>
                opts.Configuration.ChannelPrefix = RedisChannel.Literal("cmc"));

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
