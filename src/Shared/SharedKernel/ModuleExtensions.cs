using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SharedKernel;

public static class ModuleExtensions
{
    public static IServiceCollection AddModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        where TModule : IModule
    {
        TModule.AddModule(services, configuration, environment);
        return services;
    }
}
