using ServerKernel;
using Todos.Application;
using Todos.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Todos.Api;

/// <summary>
/// Kapselt die vollständige Service-Registration des Todos-Moduls.
/// </summary>
public sealed class TodosModule : IModule, IMigrateModule
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

    public static Task MigrateAsync(IServiceProvider serviceProvider)
        => serviceProvider.MigrateTodosDatabaseAsync();
}
