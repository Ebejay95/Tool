using ImportExport.Application;
using ImportExport.Infrastructure;
using ServerKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImportExport.Api;

/// <summary>
/// Registriert das gesamte ImportExport-Modul (Application + Infrastructure).
/// Die Registry-Initialisierung (domainspezifische Assemblies) erfolgt über
/// <see cref="ImportExportRegistryStartup"/> nach dem App-Build.
/// </summary>
public sealed class ImportExportModule : IModule, IMigrateModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration     configuration,
        IHostEnvironment   environment)
    {
        services.AddImportExportApplication();
        services.AddImportExportInfrastructure(configuration);
        return services;
    }

    public static Task MigrateAsync(IServiceProvider serviceProvider)
        => serviceProvider.MigrateImportExportDatabaseAsync();
}
