using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Ermöglicht EF Core Tools (dotnet ef migrations) das Erstellen von IdentityDbContext
/// ohne laufende Anwendung und ohne Verbindung zur Datenbank.
/// Wird ausschließlich zur Design-Zeit verwendet, nicht zur Laufzeit.
/// </summary>
internal sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_placeholder;Username=x;Password=x",
                npgsql => npgsql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName))
            .Options;

        return new IdentityDbContext(options);
    }
}
