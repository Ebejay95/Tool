using ImportExport.Application.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace ImportExport.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImportExportApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddSingleton<ExportableEntityRegistry>();

        return services;
    }
}
