using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel;
using Taxonomy.Application;
using Taxonomy.Infrastructure;

namespace Taxonomy.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Taxonomy-Moduls.
/// Aufruf erfolgt automatisch via ModuleDiscovery in Program.cs.
/// </summary>
public sealed class TaxonomyModule : IModule
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
}
