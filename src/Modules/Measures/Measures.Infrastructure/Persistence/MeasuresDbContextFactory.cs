using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Measures.Infrastructure.Persistence;

/// <summary>
/// Ermöglicht EF Core Tools (dotnet ef migrations) das Erstellen von MeasuresDbContext
/// ohne laufende Anwendung und ohne Verbindung zur Datenbank.
/// Wird ausschließlich zur Design-Zeit verwendet, nicht zur Laufzeit.
/// </summary>
internal sealed class MeasuresDbContextFactory : IDesignTimeDbContextFactory<MeasuresDbContext>
{
    public MeasuresDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MeasuresDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_placeholder;Username=x;Password=x",
                npgsql => npgsql.MigrationsAssembly(typeof(MeasuresDbContext).Assembly.FullName))
            .Options;

        return new MeasuresDbContext(options);
    }
}
