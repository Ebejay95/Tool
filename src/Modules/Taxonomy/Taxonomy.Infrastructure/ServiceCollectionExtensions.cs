using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taxonomy.Application.Ports;
using Taxonomy.Infrastructure.Outbox;
using Taxonomy.Infrastructure.Persistence;
using Taxonomy.Infrastructure.Repositories;
using Taxonomy.Infrastructure.Services;

namespace Taxonomy.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxonomyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TaxonomyDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Taxonomy database connection string not found");

        services.AddDbContext<TaxonomyDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(TaxonomyDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<ITaxonomyUnitOfWork>(sp => sp.GetRequiredService<TaxonomyDbContext>());

        // Repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Query Services
        services.AddScoped<ICategoryQueryService, CategoryQueryService>();
        services.AddScoped<ITagQueryService, TagQueryService>();

        // Outbox Processor
        services.AddHostedService<TaxonomyOutboxProcessor>();

        return services;
    }

    public static async Task<IServiceProvider> MigrateTaxonomyDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaxonomyDbContext>();
        await context.Database.MigrateAsync();
        return serviceProvider;
    }
}
