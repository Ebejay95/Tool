using CMC.Todos.Application;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Todos.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTodosModule(this IServiceCollection services)
    {
        services.AddScoped<ITodoService, TodoService>();
        return services;
    }
}
