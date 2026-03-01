using SharedKernel;
using Todos.Application.Ports;
using Todos.Infrastructure.Outbox;
using Todos.Infrastructure.Persistence;
using Todos.Infrastructure.Repositories;
using Todos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Todos.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodosInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("TodosDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Todos database connection string not found");

        services.AddDbContext<TodosDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(TodosDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<ITodosUnitOfWork>(provider => provider.GetRequiredService<TodosDbContext>());

        // Repositories
        services.AddScoped<ITodoRepository, TodoRepository>();

        // Query Services
        services.AddScoped<ITodoQueryService, TodoQueryService>();

        // Outbox Processor: dispatcht Domain-Events nach DB-Commit (at-least-once)
        services.AddHostedService<TodosOutboxProcessor>();

        return services;
    }

    public static async Task<IServiceProvider> MigrateTodosDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TodosDbContext>();
        await context.Database.MigrateAsync();
        return serviceProvider;
    }
}
