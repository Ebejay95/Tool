using ImportExport.Application.UseCases;
using Measures.Application.Ports;
using Measures.Infrastructure.ImportExport;
using Measures.Infrastructure.Outbox;
using Measures.Infrastructure.Persistence;
using Measures.Infrastructure.Repositories;
using Measures.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Measures.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeasuresInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MeasuresDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Measures database connection string not found");

        services.AddDbContext<MeasuresDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(MeasuresDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<IMeasuresUnitOfWork>(provider => provider.GetRequiredService<MeasuresDbContext>());

        // Repositories
        services.AddScoped<IMeasureRepository, MeasureRepository>();

        // Query Services
        services.AddScoped<IMeasureQueryService, MeasureQueryService>();

        // Import/Export
        services.AddScoped<IExportSource, MeasureExportSource>();
        services.AddScoped<IImportAdapter, MeasureImportAdapter>();

        // Outbox Processor
        services.AddHostedService<MeasuresOutboxProcessor>();

        return services;
    }

    public static async Task<IServiceProvider> MigrateMeasuresDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MeasuresDbContext>();
        await context.Database.MigrateAsync();
        return serviceProvider;
    }
}
