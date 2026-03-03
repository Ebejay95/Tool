using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerKernel;
using Taxonomy.Application;
using Taxonomy.Infrastructure;

namespace Taxonomy.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Taxonomy-Moduls.
/// </summary>
public sealed class TaxonomyModule : IModule, IMigrateModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddTaxonomyApplication();
        services.AddTaxonomyInfrastructure(configuration);
        return services;
    }

    public static Task MigrateAsync(IServiceProvider serviceProvider)
        => serviceProvider.MigrateTaxonomyDatabaseAsync();
}
