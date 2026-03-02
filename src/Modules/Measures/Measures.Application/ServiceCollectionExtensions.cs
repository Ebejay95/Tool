using Microsoft.Extensions.DependencyInjection;

namespace Measures.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeasuresApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        return services;
    }
}
