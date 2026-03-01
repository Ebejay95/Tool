using SharedKernel;
using Todos.Application;
using Todos.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Todos.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Todos-Moduls.
/// Aufruf in Program.cs: builder.Services.AddModule&lt;TodosModule&gt;(config, env)
/// </summary>
public sealed class TodosModule : IModule
{
    public static IServiceCollection AddModule(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddTodosApplication();
        services.AddTodosInfrastructure(configuration);

        return services;
    }
}
