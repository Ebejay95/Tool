using Microsoft.Extensions.DependencyInjection;

namespace Taxonomy.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxonomyApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        return services;
    }
}
