using Microsoft.Extensions.DependencyInjection;

namespace CMC.Todos.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodosApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        return services;
    }
}
