using Identity.Application;
using Identity.Infrastructure;
using SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Identity-Moduls.
/// Aufruf in Program.cs: builder.Services.AddModule&lt;IdentityModule&gt;(config, env)
/// </summary>
public sealed class IdentityModule : IModule
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
}
