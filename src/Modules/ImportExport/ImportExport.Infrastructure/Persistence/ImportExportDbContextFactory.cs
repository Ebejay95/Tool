using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ImportExport.Infrastructure.Persistence;

internal sealed class ImportExportDbContextFactory : IDesignTimeDbContextFactory<ImportExportDbContext>
{
    public ImportExportDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ImportExportDbContext>()
            .UseNpgsql("Host=localhost;Database=tool;Username=tool;Password=tool")
            .Options;
        return new ImportExportDbContext(options);
    }
}
