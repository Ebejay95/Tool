using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Taxonomy.Infrastructure.Persistence;

/// <summary>
/// Design-Time Factory – ermöglicht dotnet ef migrations ohne laufende App.
/// </summary>
internal sealed class TaxonomyDbContextFactory : IDesignTimeDbContextFactory<TaxonomyDbContext>
{
    public TaxonomyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaxonomyDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_placeholder;Username=x;Password=x",
                npgsql => npgsql.MigrationsAssembly(typeof(TaxonomyDbContext).Assembly.FullName))
            .Options;

        return new TaxonomyDbContext(options);
    }
}
