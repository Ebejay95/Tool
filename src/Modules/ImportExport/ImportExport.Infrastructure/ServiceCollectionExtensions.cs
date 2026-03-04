using ImportExport.Application.Channels;
using ImportExport.Application.Ports;
using ImportExport.Application.Registry;
using ImportExport.Infrastructure.Channels;
using ImportExport.Infrastructure.Persistence;
using ImportExport.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImportExport.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportExportInfrastructure(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        var connectionString = configuration.GetConnectionString("ImportExportDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ImportExport database connection string not found");

        services.AddDbContext<ImportExportDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ImportExportDbContext).Assembly.FullName)));

        services.AddScoped<IImportExportUnitOfWork>(sp => sp.GetRequiredService<ImportExportDbContext>());
        services.AddScoped<IMappingProfileRepository, MappingProfileRepository>();

        // Channels
        services.AddSingleton<IExportChannel, ExcelExportChannel>();
        services.AddSingleton<IExportChannel, CsvExportChannel>();
        services.AddSingleton<IImportChannel, ExcelImportChannel>();
        services.AddSingleton<IImportChannel, CsvImportChannel>();

        return services;
    }

    /// <summary>
    /// Lädt Assembly-Marker aller Module in die <see cref="ExportableEntityRegistry"/>.
    /// Muss nach dem Build des ServiceProviders aufgerufen werden.
    /// </summary>
    public static void InitializeExportableRegistry(
        this IServiceProvider               serviceProvider,
        IEnumerable<System.Reflection.Assembly> domainAssemblies)
    {
        var registry = serviceProvider.GetRequiredService<ExportableEntityRegistry>();
        registry.RegisterAssemblies(domainAssemblies);
    }

    public static async Task<IServiceProvider> MigrateImportExportDatabaseAsync(
        this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ImportExportDbContext>();
        await ctx.Database.MigrateAsync();
        return serviceProvider;
    }
}
