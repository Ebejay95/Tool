using Identity.Application;
using Identity.Infrastructure;
using ServerKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Identity-Moduls.
/// </summary>
public sealed class IdentityModule : IModule, IMigrateModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIdentityApplication();
        services.AddIdentityInfrastructure(configuration);
        return services;
    }

    public static Task MigrateAsync(IServiceProvider serviceProvider)
        => serviceProvider.MigrateIdentityDatabaseAsync();
}
