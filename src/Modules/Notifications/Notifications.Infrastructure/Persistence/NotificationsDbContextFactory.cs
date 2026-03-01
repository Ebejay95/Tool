using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notifications.Infrastructure.Persistence;

/// <summary>
/// Ermöglicht EF Core Tools (dotnet ef migrations) das Erstellen von NotificationsDbContext
/// ohne laufende Anwendung. Wird ausschließlich zur Design-Zeit verwendet.
/// </summary>
internal sealed class NotificationsDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_placeholder;Username=x;Password=x",
                npgsql => npgsql.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.FullName))
            .Options;

        return new NotificationsDbContext(options);
    }
}
