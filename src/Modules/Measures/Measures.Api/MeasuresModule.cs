using Measures.Application;
using Measures.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerKernel;

namespace Measures.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Measures-Moduls.
/// </summary>
public sealed class MeasuresModule : IModule, IMigrateModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddMeasuresApplication();
        services.AddMeasuresInfrastructure(configuration);
        return services;
    }

    public static Task MigrateAsync(IServiceProvider serviceProvider)
        => serviceProvider.MigrateMeasuresDatabaseAsync();
}
