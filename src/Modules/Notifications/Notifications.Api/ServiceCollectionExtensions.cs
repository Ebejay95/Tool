using Microsoft.Extensions.DependencyInjection;

namespace Notifications.Api;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registriert den NotificationsController aus diesem Assembly als ApplicationPart,
    /// damit ASP.NET Core ihn beim Controller-Scanning berücksichtigt.
    /// Aufruf in Program.cs: builder.Services.AddControllers().AddNotificationsWebControllers();
    /// </summary>
    public static IMvcBuilder AddNotificationsWebControllers(this IMvcBuilder builder)
    {
        builder.AddApplicationPart(typeof(NotificationsController).Assembly);
        return builder;
    }
}
