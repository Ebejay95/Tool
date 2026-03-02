using Measures.Application;
using Measures.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel;

namespace Measures.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Measures-Moduls.
/// Aufruf erfolgt automatisch via ModuleDiscovery in Program.cs.
/// </summary>
public sealed class MeasuresModule : IModule
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
}
